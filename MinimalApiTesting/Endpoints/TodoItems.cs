using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApiTesting.Business.Endpoints;
using MinimalApiTesting.Data;
using MinimalApiTesting.Dtos;
using MinimalApiTesting.Extensions;
using MinimalApiTesting.Models;

namespace MinimalApiTesting.Endpoints
{
    public class TodoItems : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {
            app.MapGet("/api/todoitems", GetAll)
                .WithTags("Todo Item")
                .Produces(200, typeof(IEnumerable<TodoItem>));

            app.MapGet("/api/todoitems/{id}", GetById)
                .WithTags("Todo Item")
                .Produces(200, typeof(TodoItem))
                .ProducesProblem(404)
                .ProducesValidationProblem(401);

            app.MapPost("/api/todoitems", PostItem)
                .WithTags("Todo Item")
                .Accepts<TodoItemInput>("application/json")
                .Produces(201, typeof(TodoItemOutput))
                .ProducesProblem(401)
                .ProducesValidationProblem(400);

            app.MapPut("/api/todoitems/{id}", UpdateTodoItem)
                .WithTags("Todo Item")
                .Accepts<TodoItemInput>("application/json")
                .Produces(201, typeof(TodoItemOutput))
                .ProducesProblem(404)
                .ProducesValidationProblem(401);

            app.MapDelete("/api/todoitems/{id}", DeleteItem)
                .WithTags("Todo Item")
                .Accepts<TodoItemInput>("application/json")
                .Produces(204)
                .ProducesProblem(404)
                .ProducesValidationProblem(401);

            app.MapGet("/api/todoitems/history", History)
                .Produces<TodoItemAudit>(200)
                .WithTags("EF Core Feature")
                .ProducesValidationProblem(401);
        }

        private async Task<IResult> History([FromServices] ApplicationDbContext dbContext)
        {
            return Results.Ok(await dbContext.TodoItems.TemporalAsOf(DateTime.UtcNow)
                .OrderBy(todoItem => EF.Property<DateTime>(todoItem, "PeriodStart"))
                .Select(todoItem => new TodoItemAudit
                {
                    Title = todoItem.Title,
                    IsCompleted = todoItem.IsCompleted,
                    PeriodStart = EF.Property<DateTime>(todoItem, "PeriodStart"),
                    PeriodEnd = EF.Property<DateTime>(todoItem, "PeriodEnd")
                })
                .ToListAsync());
        }

        private async Task<IResult> DeleteItem([FromServices] ApplicationDbContext dbContext, [FromQuery] int id, [FromBody]TodoItemInput todoItemInput)
        {
            var todoItem = await dbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
            if (todoItem is null)
                return Results.NotFound();

            dbContext.Remove(todoItem);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }

        private async Task<IResult> UpdateTodoItem([FromServices] ApplicationDbContext dbContext, [FromQuery] int id, TodoItemInput todoItemInput)
        {
            var todoItem = await dbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
            if (todoItem is null)
                return Results.NotFound();

            todoItem.IsCompleted = todoItemInput.IsCompleted;
            todoItem.Title = todoItemInput.Title;

            dbContext.Update(todoItem);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        }

        private async Task<IResult> PostItem(
            [FromServices] ApplicationDbContext dbContext, 
            [FromServices]IValidator<TodoItemInput> validator,  
            TodoItemInput todoItemInput)
        {
            var validationResult = await validator.ValidateAsync(todoItemInput);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.ToDictionary());
            
            var todoItem = new TodoItem
            {
                Title = todoItemInput.Title,
                IsCompleted = todoItemInput.IsCompleted,
                CreatedOn = DateTimeOffset.Now,
            };
            await dbContext.TodoItems.AddAsync(todoItem);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/api/todoitems/{todoItem.Id}", todoItem);
        }

        private async Task<IResult> GetById([FromServices] ApplicationDbContext dbContext, [FromQuery] int id)
        {
            var todoItem = await dbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
            return todoItem is not null ? Results.Ok(todoItem) : Results.NotFound();
        }

        private async Task<IResult> GetAll([FromServices] ApplicationDbContext dbContext)
        {
            return Results.Ok(await dbContext.TodoItems.ToListAsync());
        }
    }
}

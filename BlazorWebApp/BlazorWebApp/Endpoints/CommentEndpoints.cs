using Data.Models;
using Data.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebApp.Endpoints;

public static class CommentEndpoints
{

    public static void MapCommentApi(this WebApplication app)
    {

        app.MapGet("/api/comments/{*blogPostid}", async (IBlogApi api, string blogPostId) =>
        {
            return Results.Ok(await api.GetCommentsAsync(blogPostId));
        }).RequireAuthorization();
        
        app.MapPut("/api/Comments", async (IBlogApi api, [FromBody] Comment item) =>
        {

            return Results.Ok(await api.SaveCommentAsync(item));

        }).RequireAuthorization();

        app.MapDelete("/api/Comments/{*id}", async (IBlogApi api, string id) =>
        {
            await api.DeleteCommentAsync(id);
            return Results.Ok();
        }).RequireAuthorization();

    }
    
}
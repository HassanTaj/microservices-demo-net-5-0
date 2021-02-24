using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Todos.Models;

namespace Todos.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok(new List<Todo>
            {
                new Todo
                {
                   Id=1,
                   Name = "Study"
                },new Todo
                {
                   Id=2,
                   Name = "Work"
                },new Todo
                {
                   Id=3,
                   Name = "Get Old"
                }, new Todo
                {
                   Id=4,
                   Name = "Die"
                }
            });
        }
    }
}

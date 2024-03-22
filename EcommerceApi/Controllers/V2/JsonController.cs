using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class JsonController : ControllerBase
    {
        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        public class SampleData
        {
            public static List<Post> GetSamplePosts()
            {
                var posts = new List<Post>();

                for (int i = 1; i <= 100; i++)
                {
                    posts.Add(new Post
                    {
                        Id = i,
                        Title = $"Post {i}",
                        Content = $"This is the content of post {i}.",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = null
                    });
                }

                return posts;
            }
        }
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(SampleData.GetSamplePosts());
        }
    }
}

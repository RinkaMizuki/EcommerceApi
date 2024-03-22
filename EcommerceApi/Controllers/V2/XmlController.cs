using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.EcommerceV2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class XmlController : ControllerBase
    {
        public class Post
        {
            public int id { get; set; }
            public string title { get; set; }
            public string content { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime? updatedAt { get; set; }
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
                        id = i,
                        title = $"Post {i}",
                        content = $"This is the content of post {i}.",
                        createdAt = DateTime.Now,
                        updatedAt = null
                    });
                }

                return posts;
            }
        }
        [HttpGet]
        [Produces("application/xml")]
        public IActionResult Get()
        {
            return Ok(SampleData.GetSamplePosts());
        }
    }
}

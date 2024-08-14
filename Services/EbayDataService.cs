using eBayExtension.Contracts;
using eBayExtension.Models;

namespace eBayExtension.Services
{
    public class EbayDataService : IEbayDataService
    {
        HttpClient _httpClient;
        public EbayDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;   
        }
        public async Task<List<Post>> GetPostData()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Post>>("/posts");

            }
            catch (Exception e)
            {
                
                throw;
            }
        }

        public async Task<Post> GetPost(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Post>($"/posts/{id}");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Post>> GetPostbyFilter(Filter filter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Post>>($"/posts?userId={filter.UserId}");
        }
    }
}

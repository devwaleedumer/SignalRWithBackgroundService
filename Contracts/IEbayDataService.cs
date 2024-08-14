using eBayExtension.Models;

namespace eBayExtension.Contracts
{
    public interface IEbayDataService
    {
        public  Task<List<Post>> GetPostData();
        public  Task<Post> GetPost(int id);
        public  Task<IEnumerable<Post>> GetPostbyFilter(Filter filter);


    }
}

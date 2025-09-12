using System.Diagnostics.CodeAnalysis;

namespace BlogApp.Application.Constants;

[ExcludeFromCodeCoverage]
public static class CacheConstants
{
    public const string PostsPrefix = "posts";
    public const string TodosPrefix = "todos";
    public const string UserPrefix = "user";
    public const string AuthPrefix = "auth";

    public static class Keys
    {
        public static string PostById(Guid id)
        {
            return $"{PostsPrefix}:{id}";
        }

        public static string PostsList(int? page, int? pageSize)
        {
            return $"{PostsPrefix}:list:{page ?? 0}:{pageSize ?? 10}";
        }

        public static string TodoById(Guid id)
        {
            return $"{TodosPrefix}:{id}";
        }

        public static string TodosList(int? page, int? pageSize)
        {
            return $"{TodosPrefix}:list:{page ?? 0}:{pageSize ?? 10}";
        }

        public static string UserById(string userId)
        {
            return $"{UserPrefix}:{userId}";
        }

        public static string UserPosts(string userId)
        {
            return $"{UserPrefix}:{userId}:posts";
        }

        public static string RefreshToken(string token)
        {
            return $"{AuthPrefix}:refresh:{token}";
        }
    }

    public static class Expiration
    {
        public static readonly TimeSpan Posts = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Todos = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan User = TimeSpan.FromMinutes(60);
        public static readonly TimeSpan Auth = TimeSpan.FromMinutes(5);
    }
}
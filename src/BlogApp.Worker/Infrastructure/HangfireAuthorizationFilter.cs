using Hangfire.Dashboard;

namespace BlogApp.Worker.Infrastructure;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In a production environment, you should implement proper authentication
        // For now, we're allowing access to the dashboard
        // You can add authentication logic here based on your requirements
        return true;
    }
}
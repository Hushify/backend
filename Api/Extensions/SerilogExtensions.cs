using Serilog;
using Serilog.Debugging;

namespace Hushify.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, string sectionName = "Serilog")
    {
        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            SelfLog.Enable(Console.Error);

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName);
        });

        return builder;
    }
}
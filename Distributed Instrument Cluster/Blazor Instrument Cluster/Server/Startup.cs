using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.WebSockets;
using Blazor_Instrument_Cluster.Server.Worker;
using Instrument_Communicator_Library.Information_Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server {

	/// <summary>
	/// Class that sets up the services and configurations of the web system
	/// </summary>
	public class Startup {

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void configureServices(IServiceCollection services) {
			//Use controller
			services.AddControllers();
			services.AddRazorPages();
			services.AddResponseCompression(opts => {
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			//Add Connection tracker
			services.AddSingleton<IRemoteDeviceConnections, RemoteDeviceConnection>();
			//Start Connection listeners as background services
			services.AddHostedService<VideoListenerService>();
			services.AddHostedService<CrestronListenerService>();
			//Add singletons for socket handling
			services.AddSingleton<IVideoSocketHandler, VideoWebsocketHandler<VideoFrame>>();
			services.AddSingleton<ICrestronSocketHandler, CrestronWebsocketHandler>();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void configure(IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseResponseCompression();
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			} else {
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			//Websocket setup
			var webSocketOptions = new WebSocketOptions() {
				KeepAliveInterval = TimeSpan.FromSeconds(120),
			};

			app.UseWebSockets(webSocketOptions);

			//Do this when a web socket connects
			app.Use(async (context, next) => {
				if (context.Request.Path == "/videoStream") {
					if (context.WebSockets.IsWebSocketRequest) {
						using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) {
							var socketFinishedTcs = new TaskCompletionSource<object>();

							VideoWebsocketHandler<VideoFrame> videoWebsocketHandler =
								(VideoWebsocketHandler<VideoFrame>)app.ApplicationServices.GetService<IVideoSocketHandler>();

							videoWebsocketHandler.StartWebSocketVideoProtocol(webSocket, socketFinishedTcs);
							await socketFinishedTcs.Task;
						}
					} else {
						context.Response.StatusCode = 400;
					}
				} else if (context.Request.Path == "/crestronControl") {
					if (context.WebSockets.IsWebSocketRequest) {
						using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) {
							var socketFinishedTcs = new TaskCompletionSource<object>();

							CrestronWebsocketHandler crestronWebsocketHandler = (CrestronWebsocketHandler)app.ApplicationServices.GetService<ICrestronSocketHandler>();
							crestronWebsocketHandler.StartCrestronWebsocketProtocol(webSocket, socketFinishedTcs);

							await socketFinishedTcs.Task;
						}
					} else {
						context.Response.StatusCode = 400;
					}
				} else {
					await next();
				}
			});

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();
			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}
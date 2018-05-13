using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace AspnetCore.OAuth2.Github
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = "GitHub";

            }).AddCookie()
            .AddOAuth("GitHub",opt=> {
                opt.ClientId = Configuration["GitHub:clientId"];
                opt.ClientSecret = Configuration["GitHub:secret"];
                opt.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/signin-github");
          
                opt.AuthorizationEndpoint = Configuration["GitHub:authorizationEndpoint"];
                opt.TokenEndpoint = Configuration["GitHub:tokenEndpoint"];
                opt.UserInformationEndpoint = "https://api.github.com/user";
              
                //Claim action 1
                opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                //Claim action 2
                opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                //Claim action 3
                opt.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                //Claim action 4
                opt.ClaimActions.MapJsonKey("urn:github:public_repos:count", "public_repos");
                //Claim action 5
                opt.ClaimActions.MapJsonKey("urn:github:repos_url", "repos_url");
                
                opt.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnCreatingTicket = async context => {

                        var httpClient = context.Backchannel;
                        //http call user endpoint to get more user info
                        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await httpClient.GetAsync(context.Options.UserInformationEndpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            var userJsonString = response.Content.ReadAsStringAsync();
                            var userJObj = JObject.Parse(await response.Content.ReadAsStringAsync());

                            //This will populate claims on ClaimsIdentity as per claim action 1 to 5 above.
                            context.RunClaimActions(userJObj);
                           
                        }
                        
                    }
                };

            })
            
            ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Controller;
using WebApp.Services;
using WebApp.Middleware;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace WebApp
{ 
    public class Startup
    {
        private IServiceCollection _services;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IListProductName, PhoneName>();               //  đăng ký dịch vụ, đối tượng chỉ tạo một lần (cận thận)
            services.AddTransient<LaptopName, LaptopName>();                    //  đăng ký dịch vụ, tạo mới  mỗi lần  triệu gọi
            services.AddTransient<ProductController, ProductController>();

            services.AddDistributedMemoryCache();
            services.AddSession(cfg => {
                cfg.Cookie.Name = "xuanthulab";             // Đặt tên Sesseon - tên này sử dụng ở Browser (Cookie)
                cfg.IdleTimeout = new TimeSpan(0,60, 0);    // Thời gian tồn tại của Cookie
            });

            _services = services;


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(); 
            app.UseSession();

            app.UseCheckAccess();

            var a = app.ApplicationServices.GetService<IConfiguration>();

            app.Map("/Product", appProduct => {

                appProduct.Run(async (context) => {
                    await appProduct.ApplicationServices.GetService<ProductController>().List(context);
                });

            }); 

            

            app.Map("/RequestInfo", app01 => {
                app01.Run(async (context) => {
                    string menu         = HtmlHelper.MenuTop(HtmlHelper.DefaultMenuTopItems(), context.Request);
                    string requestinfo  = RequestProcess.RequestInfo(context.Request).HtmlTag("div", "container");
                    
                    string accessinfo  = ProductController.CountAccessInfo(context).HtmlTag("div", "container");

                    string html         = HtmlHelper.HtmlDocument("Thông tin Request", (menu + accessinfo + requestinfo));
                    await context.Response.WriteAsync(html);
                });
            });
             
            app.Map("/Form", app01 => {
                app01.Run(async (context) => {
                    string menu     = HtmlHelper.MenuTop(HtmlHelper.DefaultMenuTopItems(), context.Request);
                    string formhtml = await RequestProcess.FormProcess(context.Request);
                           formhtml = formhtml.HtmlTag("div", "container");
                    string html     = HtmlHelper.HtmlDocument("Form Post", (menu + formhtml));
                    await context.Response.WriteAsync(html);
                });
            });

            app.Map("/Encoding", app01 => {
                app01.Run(async (context) => {
                    string menu     = HtmlHelper.MenuTop(HtmlHelper.DefaultMenuTopItems(), context.Request);
                    string htmlec   = RequestProcess.Encoding(context.Request).HtmlTag("div", "container");
                    string html     = HtmlHelper.HtmlDocument("Encoding", (menu + htmlec)); 
                    await context.Response.WriteAsync(html);
                });
            });


            app.Map("/Cookies", app01 => {
                app01.Run(async (context) => {
                    string menu     = HtmlHelper.MenuTop(HtmlHelper.DefaultMenuTopItems(), context.Request);
                    string cookies  = RequestProcess.Cookies(context.Request, context.Response).HtmlTag("div", "container");
                     string html    = HtmlHelper.HtmlDocument("Đọc / Ghi Cookies", (menu + cookies));
                    await context.Response.WriteAsync(html);
                });
            });
            

            app.Map("/Json", app01 => {
                app01.Run(async (context) => {
                    string Json  = RequestProcess.GetJson();
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(Json);
                });
            });
            
            // add this if you want to add this for a particular path in an existing app
            app.Map("/allservices", builder => builder.Run(async context =>
            {
                var sb = new StringBuilder();
                sb.Append("<h1>All Services</h1>");
                sb.Append("<table><thead>");
                sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
                sb.Append("</thead><tbody>");
                foreach(var svc in _services)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{svc.ServiceType.FullName}</td>");
                    sb.Append($"<td>{svc.Lifetime}</td>");
                    sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));


            app.Run(async (HttpContext context) =>
            {
                string menu     = HtmlHelper.MenuTop(HtmlHelper.DefaultMenuTopItems(),context.Request);
                string content  = HtmlHelper.HtmlTrangchu();
                string html     = HtmlHelper.HtmlDocument("Trang chủ", menu + content);
                await context.Response.WriteAsync(html);
            }); 
        }
    }
}
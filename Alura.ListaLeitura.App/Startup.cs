using Alura.ListaLeitura.App.Repositorio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Alura.ListaLeitura.App.Negocio;
using System;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
using Microsoft.AspNetCore.Http.Extensions;
using System.IO;

namespace Alura.ListaLeitura.App
{

    public class Startup
    {

        ILivroRepositorio _repo;

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRouting();
        }
        public void Configure(IApplicationBuilder app)
        {
            var builder = new RouteBuilder(app);
            builder.MapRoute("Livros/ParaLer", LivrosParaLer);
            builder.MapRoute("Livros/Lendo", LivrosLendo);
            builder.MapRoute("Livros/Lidos", LivrosLidos);
            builder.MapRoute("Livros/Cadastrar/{titulo}/{autor}", NovoLivroParaLer);
            builder.MapRoute("Livros/Detalhes/{id}", ExibeDetalhes);
            builder.MapRoute("Cadastro/NovoLivro", CadastrarNovoLivro);
            builder.MapRoute("Livros/Cadastrar/", NovoLivroParaLer);

            app.UseRouter(builder.Build());
            //app.Run(LivrosParaLer);
        }

        private Task CadastrarNovoLivro(HttpContext context)
        {

            return context.Response.WriteAsync(CarregarArquivoHTML("Form.html"));

        }
        private string CarregarArquivoHTML(string nomeDoArquivo)
        {
            try
            {
                var data = File.ReadAllText($"../../../HTML/{nomeDoArquivo}");
                return data;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
        private Task ExibeDetalhes(HttpContext context)
        {
            Console.WriteLine("ExibeDetalhes");

            try
            {
                _repo = new LivroRepositorioCSV();
                int id = Convert.ToInt32(context.GetRouteValue("id"));
                Func<Livro, bool> fn = (l) => { return l.Id == id; };
                var livro = _repo.Todos.First(fn);
                Console.WriteLine(livro.Detalhes());
                return context.Response.WriteAsync(livro.Detalhes());
            }
            catch (InvalidOperationException e)
            {
                return context.Response.WriteAsync(e.Message);
            }
            catch (FormatException e)
            {
                return context.Response.WriteAsync("Invalid resource id");

            }


        }

        private Task NovoLivroParaLer(HttpContext context)
        {
            Console.WriteLine("NovoLivroParaLer");

            _repo = new LivroRepositorioCSV();
            if (context.GetRouteValue("titulo") != null && context.GetRouteValue("autor") != null)
            {
                var livro = new Livro()
                {

                    Titulo = Convert.ToString(context.GetRouteValue("titulo")),
                    Autor = Convert.ToString(context.GetRouteValue("autor"))

                };
                _repo.Incluir(livro);
                return context.Response.WriteAsync("O livro foi carregado com sucesso");

            }
            else if (
                    context.Request.Form["autor"].ToString() != null &&
                    context.Request.Form["titulo"].ToString() != null
                    )
            {

                _repo.Incluir(new Livro()
                {
                    Autor = context.Request.Form["autor"].ToString(),
                    Titulo = context.Request.Form["titulo"].ToString()
                });
                return context.Response.WriteAsync("O livro foi carregado com sucesso");

            }
            else
            {

                return context.Response.WriteAsync("Impossivel processar o pedido de cadastro");

            }





        }

        public Task LivrosParaLer(HttpContext context)
        {
            _repo = new LivroRepositorioCSV();

            return context.Response.WriteAsync(_repo.ParaLer.ToString());
        }
        public Task LivrosLendo(HttpContext context)
        {
            _repo = new LivroRepositorioCSV();

            return context.Response.WriteAsync(_repo.Lendo.ToString());

        }

        public Task LivrosLidos(HttpContext context)
        {
            _repo = new LivroRepositorioCSV();

            return context.Response.WriteAsync(_repo.Lidos.ToString());

        }
    }
}
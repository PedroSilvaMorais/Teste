using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            BoCliente bo = new BoCliente();
            
            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                var ValidaCPF = IsCpf(model.CPF);

                if (ValidaCPF == "")
                {
                    ValidaCPF = VerificaCPFCadastrado(model.CPF);

                    if (ValidaCPF == "")
                    {
                        model.Id = bo.Incluir(new Cliente()
                        {
                            CEP = model.CEP,
                            Cidade = model.Cidade,
                            Email = model.Email,
                            Estado = model.Estado,
                            Logradouro = model.Logradouro,
                            Nacionalidade = model.Nacionalidade,
                            Nome = model.Nome,
                            Sobrenome = model.Sobrenome,
                            Telefone = model.Telefone,
                            CPF = model.CPF
                        });


                        return Json("Cadastro efetuado com sucesso");
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        return Json(ValidaCPF);
                    }
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(ValidaCPF);
                }
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            BoCliente bo = new BoCliente();
       
            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {

                var ValidaCPF = IsCpf(model.CPF);

                if (ValidaCPF == "")
                {
                   
                    bo.Alterar(new Cliente()
                    {
                        Id = model.Id,
                        CEP = model.CEP,
                        Cidade = model.Cidade,
                        Email = model.Email,
                        Estado = model.Estado,
                        Logradouro = model.Logradouro,
                        Nacionalidade = model.Nacionalidade,
                        Nome = model.Nome,
                        Sobrenome = model.Sobrenome,
                        Telefone = model.Telefone,
                        CPF = model.CPF
                    });

                    return Json("Cadastro alterado com sucesso");
                   
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(ValidaCPF);
                }
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = cliente.CPF,
                };

            
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        public static string IsCpf(string pCPF)
        {
            string retorno = string.Empty;
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string temCpf;
            string digito;
            int soma;
            int resto;

            pCPF = pCPF.Trim();
            pCPF = pCPF.Replace(".", "").Replace("-", "");

            if (pCPF.Length != 11)
            {
                return "CPF Inválido !";
            }

            temCpf = pCPF.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(temCpf[i].ToString()) * multiplicador1[i];
            }

            resto = soma % 11;

            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            temCpf = temCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(temCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;

            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = digito + resto.ToString();

            if ( !pCPF.EndsWith(digito) || pCPF == "00000000000")
            {
                retorno = "CPF Inválido !";
            }

            return retorno;
        }

        public static string VerificaCPFCadastrado(string pCPF)
        {
            string retorno = string.Empty;

            List<Cliente> clientes = new BoCliente().PesquisaCPF(pCPF);

            if (clientes.Count() > 0)
            {
                retorno = "CPF já existente !";
            }

            return retorno;
        }
    }
}
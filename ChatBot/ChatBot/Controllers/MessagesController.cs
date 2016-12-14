using ChatBot.Serialization;
using ChatBot.Services;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace ChatBot.Controllers
{
    public class MessagesController : ApiController
    {
        public async Task<Activity> Post([FromBody]Activity message)
        {
            var connector = new ConnectorClient(new Uri(message.ServiceUrl));

            var resposta = await Response(message);
            var msg = message.CreateReply(resposta, "pt-BR");

            await connector.Conversations.ReplyToActivityAsync(msg);
            return await Task.FromResult<Activity>(msg);
        }

        private static async Task<string> Response(Activity message)
        {
            Activity resposta = new Activity();
            var response = await Luis.GetResponse(message.Text);

            if (response != null)
            {
                var intent = new Intent();
                var entity = new Serialization.Entity();

                string acao = string.Empty;
                string pessoa = string.Empty;
                string agendaInf = string.Empty;
                string agendaResult = string.Empty;

                foreach (var item in response.entities)
                {
                    switch (item.type)
                    {
                        case "Pessoa":
                            pessoa = item.entity;
                            break;
                        case "AgendaInf":
                            agendaInf = item.entity;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(pessoa))
                {
                    if (!string.IsNullOrEmpty(agendaInf))
                        resposta = message.CreateReply($"OK! Entendi, mostrando {agendaInf} de {pessoa}");
                    else
                        resposta = message.CreateReply("Não entendi qual informação vc quer do " + pessoa + ".");
                }
                else
                    resposta = message.CreateReply("Não entendi qual a pessoa vc deseja a informação.");
            }
            return resposta.Text;
        }
    }
}

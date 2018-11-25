using LuisBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Threading.Tasks;

namespace LuisBot.Dialogs
{
    //[LuisModel("cf355725-13f6-4b64-b7d6-33583d7919e5", "90a2e1e2de074cc49a93f7fa3c450099")]
    [LuisModel("c376f36b-8bd0-42cc-94f9-9f56c245c00a", "0a1b071990ae4f118d11703a449754b9")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Desculpe, eu não entendi o que quis dizer com '{result.Query}'. Poderia ser um pouco mais claro?";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }
        
        [LuisIntent("Saudação")]
        public async Task Cumprimento(IDialogContext context, LuisResult result)
        {
            string message = $"Fala ai, posso te ajudar a descobrir se existe alguma conta pendente, se quiser.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Consultar")]
        public async Task Agendamento(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, vamos lá...");

            var form = new ConsultaForm();

            //Build AgendamentoForm
            var agendamentoFormDialog = new FormDialog<ConsultaForm>(form, this.BuildAgendamentoForm, FormOptions.PromptInStart, result.Entities);

            //Show Result
            context.Call(agendamentoFormDialog, this.ShowResult);
        }

        private IForm<ConsultaForm> BuildAgendamentoForm()
        {
            OnCompletionAsyncDelegate<ConsultaForm> processBooking = async (context, state) =>
            {
                var message = "Já estou consultando...";
                await context.PostAsync(message);
            };

            return new FormBuilder<ConsultaForm>()
                .Field(nameof(ConsultaForm.TipoConta), (state) => string.IsNullOrEmpty(state.TipoConta))
                .Field(nameof(ConsultaForm.Numero), (state) => string.IsNullOrEmpty(state.Numero))
                .OnCompletion(processBooking)
                .Build();
        }

        private async Task ShowResult(IDialogContext context, IAwaitable<ConsultaForm> result)
        {
            try
            {
                var searchQuery = await result;

                var tipoAgendamento = searchQuery.TipoConta;
                var data = searchQuery.Numero;

                var message = $"Pelo que consultei aqui, para sua conta de {tipoAgendamento} existe uma pendência de R$180,00 para ser paga até {data}! Abraços!";
                await context.PostAsync(message);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Você cancelou a operação.";
                }
                else
                {
                    reply = $"Eitaaa! Alguma coisa de estranha aconteceu comigo :( Detalhes técnicos: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }
    }
}
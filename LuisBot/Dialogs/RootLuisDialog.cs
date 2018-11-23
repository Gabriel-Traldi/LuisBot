using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Threading.Tasks;

namespace LuisBot.Dialogs
{
    [LuisModel("cf355725-13f6-4b64-b7d6-33583d7919e5", "90a2e1e2de074cc49a93f7fa3c450099")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Desculpe, eu ainda não entendo sobre '{result.Query}'. Mas se quiser agendar um horário, é comigo mesmo!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Cumprimento")]
        public async Task Cumprimento(IDialogContext context, LuisResult result)
        {
            string message = $"Olá! Que tal agendar um horário conosco? Talvez um corte, ou uma tintura!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Agendamento")]
        public async Task Agendamento(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, vamos lá...");

            var form = new AgendamentoForm();

            //Build AgendamentoForm
            var agendamentoFormDialog = new FormDialog<AgendamentoForm>(form, this.BuildAgendamentoForm, FormOptions.PromptInStart, result.Entities);

            //Show Result
            context.Call(agendamentoFormDialog, this.ShowResult);
        }

        private IForm<AgendamentoForm> BuildAgendamentoForm()
        {
            OnCompletionAsyncDelegate<AgendamentoForm> processBooking = async (context, state) =>
            {
                var message = "Obrigado pelo contato, estamos prestes a agendar seu horário...";
                await context.PostAsync(message);
            };

            return new FormBuilder<AgendamentoForm>()
                .Field(nameof(AgendamentoForm.TipoAgendamento), (state) => string.IsNullOrEmpty(state.TipoAgendamento))
                .Field(nameof(AgendamentoForm.Data), (state) => string.IsNullOrEmpty(state.Data))
                .OnCompletion(processBooking)
                .Build();
        }

        private async Task ShowResult(IDialogContext context, IAwaitable<AgendamentoForm> result)
        {
            try
            {
                var searchQuery = await result;

                var tipoAgendamento = searchQuery.TipoAgendamento;
                var data = searchQuery.Data;

                var message = $"Seu horário para realizar {tipoAgendamento} foi agendado para {data}! Estaremos lhe aguardando!";
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
                    reply = $"Oops! Alguma de errado ocorreu :( Detalhes técnicos: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        [Serializable]
        public class AgendamentoForm
        {
            [Prompt("O que você gostaria de fazer no seu cabelo? {||}", AllowDefault = BoolDefault.True)]
            [Describe("Tipo, exemplo: tintura")]
            public string TipoAgendamento { get; set; }

            [Prompt("Para quando você gostaria de agendar? {||}", AllowDefault = BoolDefault.True)]
            [Describe("Data, exemplo: amanhã, próxima semana ou qualquer data, como: 12-06-2018")]
            public string Data { get; set; }
        }
    }
}
using Microsoft.Bot.Builder.FormFlow;
using System;

namespace LuisBot.Models
{
    [Serializable]
    public class ConsultaForm
    {
        [Prompt("Qual o tipo de conta você gostaria de consultar? {||}", AllowDefault = BoolDefault.True)]
        [Describe("Tipo, exemplo: água")]
        public string TipoConta { get; set; }

        [Prompt("Qual o número de registro da casa? {||}", AllowDefault = BoolDefault.True)]
        [Describe("Numero, exemplo: 123")]
        public string Numero { get; set; }
    }
}
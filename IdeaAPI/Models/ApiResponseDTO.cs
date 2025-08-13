using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdeaAPI.Models
{
    internal class ApiResponseDTO
    {
        internal object IdeaId;

        [JsonPropertyName("msg")]

        public string? Msg { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyOrder(0)]
        public object client { get; private set; }
    }
}

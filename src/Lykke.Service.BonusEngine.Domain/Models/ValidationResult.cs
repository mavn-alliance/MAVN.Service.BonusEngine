using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class ValidationResult
    {
        public bool IsValid => ValidationMessages == null || !ValidationMessages.Any();

        public List<string> ValidationMessages { get; set; }

        public ValidationResult()
        {
            ValidationMessages = new List<string>();
        }

        public void Add(params string[] messages)
        {
            ValidationMessages.AddRange(messages);
        }

        public void Add(ValidationResult result)
        {
            ValidationMessages.AddRange(result.ValidationMessages);
        }
    }
}

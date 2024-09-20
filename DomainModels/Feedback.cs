using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
	public class Feedback
	{
		public int FeedBackId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public string FeedbackText { get; set; } = null!;
	}
}

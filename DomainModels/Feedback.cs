using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
	public class Feedback
	{
		public int FeedBackId { get; set; }
        public int UserId { get; set; }
        public string FeedbackText { get; set; } = null!;
	}
}

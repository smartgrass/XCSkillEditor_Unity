using System;

namespace NaughtyAttributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class ShowIfAttribute : ShowIfAttributeBase
	{
		public ShowIfAttribute(string condition, string showMsg =null)
			: base(condition)
		{
			Inverted = false;
			ShowMsg = showMsg;
		}

		public ShowIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
			: base(conditionOperator, conditions)
		{
			Inverted = false;
		}
	}
}

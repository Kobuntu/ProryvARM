using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Microsoft.VisualBasic.Activities;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class BaseAlarmActivity : BaseArmActivity<bool>
    {
        public BaseAlarmActivity()
        {
            WorkflowActivity_ID = new InArgument<int>(new VisualBasicValue<int>(ActivitiesSettings.InParamNameWorkFlowId));
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Идентификатор процессса")]
        public InArgument<int> WorkflowActivity_ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;

namespace Proryv.Workflow.Activity.ARM
{
    public class InsertToExplUserJournal : BaseArmActivity<bool>
    {
        public InsertToExplUserJournal()
        {
            this.DisplayName = "Сообщение в журнал действий пользователей";
        }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("User_ID")]
        [RequiredArgument]
        public InArgument<string> User_ID { get; set; }
        

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("EventString")]
        [RequiredArgument]
        public InArgument<string> EventString { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("ApplicationType")]
        [RequiredArgument]
        public InArgument<byte> ApplicationType { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("CommentString")]
        [RequiredArgument]
        public InArgument<string> CommentString { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("ParentObjectID")]
        [RequiredArgument]
        public InArgument<string> ParentObjectID { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("ParentObjectName")]
        [RequiredArgument]
        public InArgument<string> ParentObjectName { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("ObjectID")]
        [RequiredArgument]
        public InArgument<string> ObjectID { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("ObjectName")]
        [RequiredArgument]
        public InArgument<string> ObjectName { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("EventType")]
        [RequiredArgument]
        public InArgument<byte> EventType { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            //Login не возвращает ID пока пишем в user_ID логин и ставим галку 
            string user_ID = User_ID.Get(context);
            string eventSting = EventString.Get(context);
            
            byte applicationType = ApplicationType.Get(context);
            string commentString = CommentString.Get(context);
            string parentObjectID = ParentObjectID.Get(context);
            string parentObjectName = ParentObjectName.Get(context);
            string objectID = ObjectID.Get(context);
            string bjectName = ObjectName.Get(context);
            byte eventType = EventType.Get(context);

            try
            {
                DeclaratorService.Insert_ExplUserJournal(user_ID, eventSting, commentString, applicationType, 0, eventType, bjectName, objectID, parentObjectName, parentObjectID,
                    true);
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
            }
            

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}

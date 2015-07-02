﻿using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.Threading;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    public class ServiceInstanceEditorResource :INotifyPropertyChanged
    {
        private readonly ObservableCollection<ListAllServicesResponse> serviceTypes = new ObservableCollection<ListAllServicesResponse>();

        private readonly ObservableCollection<ListAllServicePlansResponse> servicePlans = new ObservableCollection<ListAllServicePlansResponse>();

        private ErrorResource errorResource = new ErrorResource();

        internal ErrorResource Error
        {
            get { return errorResource; }
            set { this.errorResource = value; }
        }

        public ObservableCollection<ListAllServicesResponse> ServiceTypes
        {
            get
            {
                return this.serviceTypes;
            }
        }

        public ObservableCollection<ListAllServicePlansResponse> ServicePlans
        {
            get
            {
                return this.servicePlans;
            }
        }

        private void EnterInit()
        {
            this.Error.HasErrors = false;
            this.Error.ErrorMessage = string.Empty;
        }

        private void ExitInit()
        {
            this.ExitInit(null);
        }

        private void ExitInit(Exception error)
        {
            this.Error.HasErrors = error != null;
            if (this.Error.HasErrors)
            {
                List<string> errors = new List<string>();
                ErrorFormatter.FormatExceptionMessage(error, errors);
                StringBuilder sb = new StringBuilder();
                foreach (string errorLine in errors)
                {
                    sb.AppendLine(errorLine);
                }

                this.Error.ErrorMessage = sb.ToString();
            }
        }


        public ServiceInstanceEditorResource(CloudFoundryClient client)
        {
                InitServicesInformation(client);
        }

        private void OnUIThread(Action action)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.Generic.Invoke(action);
        }


        private void InitServicesInformation(CloudFoundryClient client)
        {
            Task.Run(async () =>
            {
                this.EnterInit();

                var services = await client.Services.ListAllServices();
                foreach (var service in services)
                {
                    OnUIThread(() => { ServiceTypes.Add(service); });
                }

                var plans = await client.ServicePlans.ListAllServicePlans();
                foreach (var plan in plans)
                {
                    OnUIThread(() => { ServicePlans.Add(plan); });
                }
            }).ContinueWith((antecedent) =>
            {
                if (antecedent.Exception != null)
                {
                    this.ExitInit(antecedent.Exception);
                }
                else
                {
                    this.ExitInit();
                }
            }).Forget();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
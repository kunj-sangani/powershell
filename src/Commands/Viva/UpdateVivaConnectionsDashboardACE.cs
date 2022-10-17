﻿using PnP.Core.Model.SharePoint;

using System;
using System.Linq;
using System.Text.Json;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.Viva
{
    [Cmdlet(VerbsData.Update, "PnPVivaConnectionsDashboardACE", DefaultParameterSetName = ParameterSet_TYPEDPROPERTIES)]
    [OutputType(typeof(IVivaDashboard))]
    public class UpdateVivaConnectionsACE : PnPWebCmdlet
    {
        private const string ParameterSet_JSONProperties = "Update using JSON properties";
        private const string ParameterSet_TYPEDPROPERTIES = "Update using typed properties";

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public Guid Identity;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public string Title;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        public string PropertiesJSON;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public object Properties;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public string Description;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public string IconProperty;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public int Order;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_JSONProperties)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TYPEDPROPERTIES)]
        public CardSize CardSize = CardSize.Medium;

        protected override void ExecuteCmdlet()
        {
            if (PnPContext.Site.IsHomeSite())
            {
                IVivaDashboard dashboard = PnPContext.Web.GetVivaDashboardAsync().GetAwaiter().GetResult();
                var aceToUpdate = dashboard.ACEs.FirstOrDefault(p => p.InstanceId == Identity);

                if (aceToUpdate != null)
                {
                    bool updateRequired = false;
                    if (ParameterSpecified(nameof(Title)))
                    {
                        aceToUpdate.Title = Title;
                        updateRequired = true;
                    }

                    if (ParameterSpecified(nameof(PropertiesJSON)))
                    {
                        aceToUpdate.Properties = JsonSerializer.Deserialize<JsonElement>(PropertiesJSON);
                        updateRequired = true;
                    }

                    if (ParameterSpecified(nameof(Properties)))
                    {
                        // Serialize the properties object to JSON so that the JsonPropertyName attributes get applied for correct naming and casing and then assign the result back
                        var serializedProperties = JsonSerializer.Serialize(Properties as CardDesignerProps);
                        aceToUpdate.Properties = serializedProperties;
                        updateRequired = true;
                    }

                    if (ParameterSpecified(nameof(Description)))
                    {
                        aceToUpdate.Description = Description;
                        updateRequired = true;
                    }

                    if (ParameterSpecified(nameof(IconProperty)))
                    {
                        aceToUpdate.IconProperty = IconProperty;
                        updateRequired = true;
                    }

                    if (ParameterSpecified(nameof(CardSize)))
                    {
                        aceToUpdate.CardSize = CardSize;
                        updateRequired = true;
                    }

                    if (updateRequired)
                    {
                        if (ParameterSpecified(nameof(Order)) && Order > -1)
                        {
                            dashboard.UpdateACE(aceToUpdate, Order);
                        }
                        else
                        {
                            dashboard.UpdateACE(aceToUpdate);
                        }

                        dashboard.Save();

                        dashboard = PnPContext.Web.GetVivaDashboardAsync().GetAwaiter().GetResult();
                        WriteObject(dashboard, true);
                    }
                }
                else
                {
                    WriteWarning("ACE with specified instance ID not found");
                }
            }
            else
            {
                WriteWarning("Connected site is not a home site");
            }
        }
    }
}
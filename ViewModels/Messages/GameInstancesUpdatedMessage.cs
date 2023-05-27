using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Messages;

public class GameInstancesUpdatedMessage : ValueChangedMessage<List<InstanceData>>
{
    public GameInstancesUpdatedMessage(List<InstanceData> value) : base(value)
    {
    }
}

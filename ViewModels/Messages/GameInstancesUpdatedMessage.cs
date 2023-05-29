using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Messages;

public class GameInstancesUpdatedMessage : ValueChangedMessage<List<InstanceData>>
{
    public GameInstancesUpdatedMessage(List<InstanceData> value) : base(value)
    {
    }
}

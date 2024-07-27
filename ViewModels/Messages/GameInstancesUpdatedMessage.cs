// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Messages;

public class GameInstancesUpdatedMessage : ValueChangedMessage<List<InstanceData>>
{
    public GameInstancesUpdatedMessage(List<InstanceData> value) : base(value)
    {
    }
}

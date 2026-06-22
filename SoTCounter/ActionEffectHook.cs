using System;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace SoTCounter;

public sealed class ActionEffectHook : IDisposable
{
    private const uint ProvokeActionId = 7533;
	private const uint SoTActionId = 11386;

	private readonly Hook<ActionEffectHandler.Delegates.Receive> hook;
    private readonly SoTTracker tracker;

    public unsafe ActionEffectHook(SoTTracker tracker, IGameInteropProvider gameInterop)
    {
        this.tracker = tracker;

        hook = gameInterop.HookFromAddress<ActionEffectHandler.Delegates.Receive>(
            ActionEffectHandler.Addresses.Receive.Value,
            OnReceiveActionEffect);
        hook.Enable();

        Plugin.Log.Information("[SoTCounter] ActionEffectHook enabled.");
    }

    private unsafe void OnReceiveActionEffect(
        uint casterEntityId,
        Character* caster,
        Vector3* targetPos,
        ActionEffectHandler.Header* header,
        ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        var actionId = header->ActionId;
        hook.Original(casterEntityId, caster, targetPos, header, effects, targetEntityIds);
        if (actionId != SoTActionId) return;
        tracker.Increment(casterEntityId);
    }

    public void Dispose()
    {
        hook.Disable();
        hook.Dispose();
    }
}

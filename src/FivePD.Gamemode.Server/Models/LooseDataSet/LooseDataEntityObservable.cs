// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CitizenFX.Core;
using FivePD.Common.Models;

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable S125
#pragma warning disable SA1512
#pragma warning disable SA1515
#pragma warning disable SA1649
#pragma warning disable SA1602

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    public class LooseDataEntityObservable
    {
        private readonly List<Action<LooseDataEntity>> _observers = new List<Action<LooseDataEntity>>();

        public LooseDataEntityObservable(Type entityType, LooseDataEntityObservableType type)
        {
            this.EntityType = entityType;
            this.Type = type;
        }

        public Type EntityType { get; }

        public LooseDataEntityObservableType Type { get; }

        public IDisposable Subscribe(Action<LooseDataEntity> observer)
        {
            this._observers.Add(observer);
            return new LooseDataEntityObservableSubscription(this, observer);
        }

        public void Unsubscribe(Action<LooseDataEntity> action)
        {
            this._observers.Remove(action);
        }

        public void InvokeObservers(LooseDataEntity entity)
        {
            foreach (var o in this._observers)
            {
                o?.Invoke(entity);
            }
        }
    }
}
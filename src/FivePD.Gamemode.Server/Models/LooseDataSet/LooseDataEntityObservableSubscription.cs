// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable S125
#pragma warning disable SA1512
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    #pragma warning disable S3881 // Conforms to pattern.
    public class LooseDataEntityObservableSubscription : IDisposable
    {
        private readonly Action<LooseDataEntity> _action;
        private LooseDataEntityObservable _subject;

        public LooseDataEntityObservableSubscription(LooseDataEntityObservable subject, Action<LooseDataEntity> action)
        {
            this._subject = subject;
            this._action = action;
        }

        public void Dispose()
        {
            this._subject.Unsubscribe(this._action);
            this._subject = null!;
        }
    }
}
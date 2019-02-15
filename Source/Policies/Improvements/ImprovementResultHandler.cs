﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Concepts;
using Concepts.Improvables;
using Concepts.Improvements;
using Dolittle.Artifacts;
using Dolittle.Collections;
using Dolittle.Domain;
using Dolittle.Events;
using Dolittle.Execution;
using Dolittle.Runtime.Commands;
using Dolittle.Runtime.Commands.Coordination;
using Dolittle.Tenancy;
using Domain.Improvements;

namespace Policies.Improvements
{

    /// <summary>
    /// 
    /// </summary>
    public class ImprovementResultHandler : IImprovementResultHandler
    {
        static ArtifactId _nullCommandArtifactId = (ArtifactId)Guid.Parse("c7d1f5cc-40bb-4cd4-b589-9cb11a43c962");

        readonly IExecutionContextManager _executionContextManager;
        readonly ICommandContextManager _commandContextManager;
        readonly IAggregateRootRepositoryFor<Improvement> _repository;

        public ImprovementResultHandler(
            IExecutionContextManager executionContextManager,
            ICommandContextManager commandContextManager,
            IAggregateRootRepositoryFor<Improvement> repository)
        {
            _executionContextManager = executionContextManager;
            _commandContextManager = commandContextManager;
            _repository = repository;
        }

        public void HandleSuccess(
            ImprovementId improvement)
        {
            SetImprovmentResult(improvement, succeeded: true);
        }
        
        public void HandleFailure(
            ImprovementId improvement)
        {
            SetImprovmentResult(improvement, succeeded: false);
        }

        void SetImprovmentResult(ImprovementId id, bool succeeded)
        {
           var request = new CommandRequest(_executionContextManager.Current.CorrelationId, _nullCommandArtifactId, ArtifactGeneration.First, new Dictionary<string,object>());
            
            using( var commandContext = _commandContextManager.EstablishForCommand(request) )
            {
                var improvement = _repository.Get(id);
                if(succeeded)
                {
                    improvement.Complete();
                }
                else 
                {
                    improvement.Fail();
                }
            }
        }
    }
}
/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
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
using Events.Improvements;

namespace Policies.Improvements
{
    /// <summary>
    /// 
    /// </summary>
    public class ImprovementStepResultHandler : IImprovementStepResultHandler
    {
        static ArtifactId _nullCommandArtifactId = (ArtifactId)Guid.Parse("c7d1f5cc-40bb-4cd4-b589-9cb11a43c962");

        readonly IExecutionContextManager _executionContextManager;
        readonly ICommandContextManager _commandContextManager;
        readonly IAggregateRootRepositoryFor<Improvement> _repository;
        readonly IImprovementContextFactory _improvementContextFactory;
        readonly IRecipeLocator _recipeLocator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContextManager"></param>
        /// <param name="repository"></param>
        /// <param name="improvementPodFactory"></param>
        public ImprovementStepResultHandler(
            IExecutionContextManager executionContextManager,
            ICommandContextManager commandContextManager,
            IAggregateRootRepositoryFor<Improvement> repository,
            IImprovementContextFactory improvementContextFactory,
            IRecipeLocator recipeLocator)
        {
            _executionContextManager = executionContextManager;
            _commandContextManager = commandContextManager;
            _repository = repository;
            _improvementContextFactory = improvementContextFactory;
            _recipeLocator = recipeLocator;
        }

        public void HandleSuccessfulStep(
            RecipeType recipeType,
            StepNumber stepNumber,
            ImprovementId improvement,
            ImprovableId improvable,
            Concepts.Version version)
        {
            var context = _improvementContextFactory.GetFor(improvable, version);
            var recipe = _recipeLocator.GetByName(recipeType);
            var steps = recipe.GetStepsFor(context).ToArray();
            var step = steps[stepNumber];
            var events = step.GetSucceededEventsFor(context);
            events = events.Concat(new[] {new StepSucceeded(stepNumber)});
            ApplyEventsFor(context, events);
        }
        
        public void HandleFailedStep(
            RecipeType recipeType,
            StepNumber stepNumber,
            ImprovementId improvement,
            ImprovableId improvable,
            Concepts.Version version)
        {
            var context = _improvementContextFactory.GetFor(improvable, version);
            var recipe = _recipeLocator.GetByName(recipeType);
            var steps = recipe.GetStepsFor(context).ToArray();
            var step = steps[stepNumber];
            var events = step.GetFailedEventsFor(context);
            events = events.Concat(new[] {new StepFailed(stepNumber)});
            ApplyEventsFor(context, events);
        }


        void ApplyEventsFor(ImprovementContext context, IEnumerable<IEvent> events)
        {
           var request = new CommandRequest(_executionContextManager.Current.CorrelationId, _nullCommandArtifactId, ArtifactGeneration.First, new Dictionary<string,object>());
            
            using( var commandContext = _commandContextManager.EstablishForCommand(request) )
            {
                var improvement = _repository.Get(context.Improvement.Id);
                events.ForEach(_ => improvement.Apply(_));
            }
        }
    }
}
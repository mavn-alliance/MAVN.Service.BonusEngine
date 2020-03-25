using Lykke.Service.BonusEngine.Domain.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CampaignModel = Lykke.Service.BonusEngine.Domain.Models.Campaign;
using Condition = Lykke.Service.BonusEngine.Domain.Models.Condition;

namespace Lykke.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignServiceBonusProcessingTests
    {
        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with one Referral condition with Completion count = x + 1
        ///  And the customer has completed the condition x times
        /// When method resolves the event
        /// Then User should be granted x times immediate rewards
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ShouldSendMultipleBonusOperation_WhenConditionFulfilledMultipleTimes(int triggerCount)
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture
            {
                ConditionModel =
                {
                    CompletionCount = triggerCount + 1
                }
            };

            fixtureData.SetupConditionProcessingMocks();

            fixtureData.SetupCampaignCacheGetCampaignsByTypeAsync();
            fixtureData.SetupConditionRewardMocks();

            // in SetupCommonMocks GetByCampaignAsync each time takes new completion entry
            for (int i = 0; i < triggerCount; i++)
            {
                fixtureData.CampaignCompletions.Add(new CampaignCompletion()
                {
                    CampaignCompletionCount = 0,
                    CampaignId = fixtureData.CampaignId,
                    CustomerId = fixtureData.CustomerId,
                });
            }

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);
            // Execute
            for (int i = 0; i < triggerCount; i++)
            {
                await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                    fixtureData.CustomerId,
                    fixtureData.PartnerId,
                    fixtureData.LocationId,
                    fixtureData.EventDataEmpty,
                    fixtureData.BonusTypeName);
            }

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(triggerCount));
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with one SignUp condition with Completion count = 1
        ///  And the condition does not have immediate reward
        ///  And the campaign has reward of 20 tokens
        /// When method resolves the event
        /// Then User should be granted 20 tokens
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldNotSendImmediateRewardBonusOperation_WhenConditionInCampaignIsCompletedWithoutImmediateReward()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture
            {
                ConditionModel =
                {
                    CompletionCount = 1,
                    ImmediateReward = 0
                }
            };

            fixtureData.SetupAllMocks();

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                IsCompleted = true
            });

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(0);
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Once);

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward != 0 && p.Reward == fixtureData.CampaignModel.Reward)));
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with one SignUp condition with completion count = 1
        ///  And the Campaign has 20 tokens reward for completing
        ///  AND the Condition of the Campaign has 10 tokens for Immediate reward
        /// When method resolves the event
        /// Then User should be granted 30 tokens(10 insta + 20 complete)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendBonusOperation_WhenBothCampaignAndConditionAreCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            fixtureData.SetupAllMocks();

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                IsCompleted = true
            });

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(2));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 10
        ///  And a Campaign is configured with another SignUp condition with Completion count = 2 and Immediate Reward = 45
        ///  And the campaign has reward of 20 tokens
        ///  And the user has not yet completed the condition
        /// When method resolves the event
        /// Then User should be granted 10 tokens for completing only the first condition
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendImmediateRewardBonusOperationOnly_WhenConditionInCampaignIsCompletedButCampaignNot()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            fixtureData.SetupAllMocks();

            const int immediateReward2 = 45;
            const string conditionId2 = "conditionId2";
            fixtureData.ConditionModels.Add(new Condition
            {
                CampaignId = fixtureData.CampaignId,
                CompletionCount = 3,
                BonusType = fixtureData.BonusType,
                Id = conditionId2,
                ImmediateReward = immediateReward2
            });

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                ConditionId = conditionId2,
                CurrentCount = 1,
                CustomerId = fixtureData.CustomerId,
                IsCompleted = false
            });

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Once);

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(
                    It.Is<BonusOperation>(p => p.Reward != immediateReward2 && p.Reward == fixtureData.ConditionModel.ImmediateReward)));
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign 1 is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 10
        ///  And a Campaign 2 is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 14
        ///  And the Campaign 1 has reward of 20 tokens
        ///  And the Campaign 2 has reward of 34 tokens
        /// When method resolves the event
        /// Then User should be granted 74 tokens for completing all conditions and campaigns
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendBonusOperation_WhenMultipleCampaignAndConditionAreAllCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            fixtureData.SetupAllMocks();

            var campaignId2 = Guid.NewGuid().ToString("D");
            const int conditionImmediateReward2 = 14;
            const int campaignReward2 = 34;

            fixtureData.CampaignModels.Add(new CampaignModel
            {
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = campaignId2,
                        CompletionCount = 2,
                        BonusType = new BonusType
                        {
                            Type =  "signup",
                         DisplayName = "SignUp"
                        },
                        Id = "conditionId2",
                        ImmediateReward = conditionImmediateReward2
                    }
                },
                Id = campaignId2,
                Name = "SignUp 2 Campaign",
                Reward = campaignReward2,
                CompletionCount = 2
            });

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CampaignCompletionCount = 0,
                CampaignId = campaignId2,
                CustomerId = fixtureData.CustomerId
            });

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                IsCompleted = true
            });

            fixtureData.BonusCalculatorServiceMock.SetupSequence(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(14)
                .ReturnsAsync(10);

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(4));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == conditionImmediateReward2)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == campaignReward2)));
        }
        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign 1 is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 10
        ///  And a Campaign 2 is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 14
        ///  And a Campaign 3 is configured with a Referral, Purchase and SignUp conditions with Completion count = 10, 5, 1 no immediate reward
        ///  And the Campaign 1 has reward of 20 tokens
        ///  And the Campaign 2 has reward of 34 tokens
        ///  And the Campaign 3 has reward of 10 tokens
        /// When method resolves the event
        /// Then User should be granted 74 tokens for completing all conditions and campaigns
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendBonusOperation_WhenMultipleCampaignAndConditionAreNotAllCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            fixtureData.SetupAllMocks();

            var campaignId2 = Guid.NewGuid().ToString("D");
            var campaignId3 = Guid.NewGuid().ToString("D");
            const int conditionImmediateReward2 = 14;
            const int campaignReward2 = 34;

            fixtureData.CampaignModels.Add(new Domain.Models.Campaign
            {
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = campaignId2,
                        CompletionCount = 2,
                        BonusType = new BonusType
                        {
                            Type =  "signup",
                            DisplayName = "SignUp"
                        },
                        Id = "conditionId2",
                        ImmediateReward = conditionImmediateReward2
                    }
                },
                Id = campaignId2,
                Name = "SignUp 2 Campaign",
                Reward = campaignReward2,
                CompletionCount = 2
            });

            fixtureData.CampaignModels.Add(new CampaignModel
            {
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = campaignId3,
                        CompletionCount = 1,
                        BonusType = new BonusType
                        {
                            Type =  "signup",
                            DisplayName = "SignUp"
                        },
                        Id = "conditionId3",
                        ImmediateReward = 0
                    },
                    new Condition
                    {
                        CampaignId = campaignId3,
                        CompletionCount = 2,
                        BonusType = new BonusType
                        {
                           Type = "referral",
                           DisplayName = "Referral"
                        },
                        Id = "conditionId4",
                        ImmediateReward = 10000
                    },
                    new Condition
                    {
                        CampaignId = campaignId3,
                        CompletionCount = 2,

                        BonusType = new BonusType
                        {
                        Type = "purchase",
                        DisplayName = "Purchase"
                        },
                        Id = "conditionId5",
                        ImmediateReward = 1500
                    }
                },
                Id = campaignId3,
                Name = "Campaign 3",
                Reward = 1000,
                CompletionCount = 10
            });

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CampaignCompletionCount = 0,
                CampaignId = campaignId2,
                CustomerId = fixtureData.CustomerId
            });

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CampaignCompletionCount = 0,
                CampaignId = campaignId3,
                CustomerId = fixtureData.CustomerId
            });

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                IsCompleted = true
            });

            fixtureData.BonusCalculatorServiceMock.SetupSequence(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10)
                .ReturnsAsync(14)
                .ReturnsAsync(0);

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(4));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == conditionImmediateReward2)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == campaignReward2)));
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 10
        ///  And the campaign has reward of 20 tokens
        ///  And the user has completed the condition
        /// When method resolves the event
        /// Then User should not be granted any tokens
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldNotSendBonusOperation_WhenConditionInCampaignIsAlreadyCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();
            fixtureData.SetupCampaignCacheGetCampaignsByTypeAsync();

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                ConditionId = fixtureData.ConditionId,
                CurrentCount = 1,
                CustomerId = fixtureData.CustomerId,
                IsCompleted = true
            });
            fixtureData.CampaignModel.Conditions = new List<Condition>()
            {
                // Condition one is completed, condition 2 is not - we use 2 conditions to simulate running campaign, since 
                // campaign completion is used to determine if a campaign is finished by a customer or not
                new Condition()
                {
                    CampaignId = fixtureData.CampaignModel.Id,
                    CompletionCount = 1,
                    BonusType = new BonusType{Type ="signup"},
                    Id = fixtureData.ConditionId,
                    ImmediateReward = 10
                },
                new Condition()
                {
                    CampaignId = fixtureData.CampaignModel.Id,
                    CompletionCount = 1,
                    BonusType = new BonusType{Type = "referral"},
                    Id = Guid.NewGuid().ToString("D"),
                    ImmediateReward = 15
                }
            };

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.ConditionCompletionServiceMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<ConditionCompletion>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Never);
            fixtureData.ConditionCompletionServiceMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<string>()), Times.Never);
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Never);
        }

        /// <summary>
        /// Given that Customer is registered
        ///  And and Event for that is dispatched
        ///  And Subscriber successfully fetches the event
        ///  And Subscriber send the customer id and event type to the Campaign management service
        ///  And a Campaign is configured with a SignUp condition with Completion count = 1 and Immediate Reward = 10
        ///  And a Campaign is configured with another SignUp condition with Completion count = 2 and Immediate Reward = 45
        ///  And the campaign has reward of 20 tokens
        ///  And the user has completed all conditions
        /// When method resolves the event
        /// Then User should not be granted any tokens
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldNotSendBonusOperation_WhenAllConditionsInCampaignIsAlreadyCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();
            fixtureData.SetupCampaignCacheGetCampaignsByTypeAsync();
            const string conditionId2 = "d9aa75ea-e0c6-4436-a9d5-bf5ab8c70e54";

            fixtureData.ConditionModels.Add(new Condition
            {
                CampaignId = fixtureData.CampaignId,
                CompletionCount = 1,
                BonusType = fixtureData.BonusType,
                Id = conditionId2,
                ImmediateReward = 45
            });

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                ConditionId = fixtureData.ConditionId,
                CurrentCount = 1,
                CustomerId = fixtureData.CustomerId,
                IsCompleted = true
            });

            fixtureData.ConditionCompletions.Add(new ConditionCompletion
            {
                ConditionId = conditionId2,
                CurrentCount = 1,
                CustomerId = fixtureData.CustomerId,
                IsCompleted = true
            });

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.ConditionCompletionServiceMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<ConditionCompletion>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Never);
            fixtureData.ConditionCompletionServiceMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<string>()), Times.Never);
            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Never);
        }

        /// <summary>
        ///  Given that Customer is registered
        ///   And and Event for that is dispatched
        ///   And Subscriber successfully fetches the event
        ///   And Subscriber send the customer id and event type to the Campaign management service
        ///   And a Campaign is configured with one SignUp condition with completion count = 3
        ///   And another Campaign is configured with one SignUp condition with completion count = 5
        ///   And the Campaign 1 has 35 tokens reward for completing
        ///   And the Campaign 2 has 45 tokens reward for completing
        ///   AND the Condition of the Campaign 1 has no tokens for Immediate reward
        ///   AND the Condition of the Campaign 2 has 30 tokens for Immediate reward
        ///  When method resolves the event
        ///  Then User should be granted 35 tokens
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendBonusOperationOnlyOnce_WhenOneOfMultipleCampaignsIsCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture
            {
                ConditionModel =
                {
                    CompletionCount = 3,
                    ImmediateReward = 0
                }
            };

            fixtureData.SetupAllMocks();

            fixtureData.ConditionModels.Add(new Condition
            {
                CampaignId = "d9aa75ea-e0c6-4436-a9d5-bf5ab8c70e54",
                CompletionCount = 5,
                BonusType = fixtureData.BonusType,
                Id = "d9aa75ea-e0c6-4436-a9d5-bf5ab8c70e54",
                ImmediateReward = 30
            });

            fixtureData.NewConditionCompletion.CurrentCount = 2;

            fixtureData.ConditionCompletions.Add(fixtureData.NewConditionCompletion);

            fixtureData.CampaignModel.Reward = 35;

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(0);
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(1));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));
        }

        /// <summary>
        ///  Given that Customer is registered
        ///   And and Event for that is dispatched
        ///   And Subscriber successfully fetches the event
        ///   And Subscriber send the customer id and event type to the Campaign management service
        ///   And a Campaign is configured with one SignUp condition with completion count = 3
        ///   And another Campaign is configured with one SignUp condition with completion count = 5
        ///   And the Campaign 1 has 35 tokens reward for completing
        ///   And the Campaign 2 has 45 tokens reward for completing
        ///   AND the Condition of the Campaign 1 has no tokens for Immediate reward
        ///   AND the Condition of the Campaign 2 has 30 tokens for Immediate reward
        ///  When method resolves the event
        ///  Then User should be granted 35 tokens
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldCompleteCampaignMultipleTimes_WhenCampaignCompletionCountIsSetToMoreThan1()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture
            {
                CampaignCompletion =
                {
                    CampaignCompletionCount = 0
                },
                CampaignModel =
                {
                    CompletionCount = 2
                }
            };

            fixtureData.CampaignCompletions.Add(fixtureData.CampaignCompletion);
            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CampaignCompletionCount = 2,
                CampaignId = fixtureData.CampaignCompletion.CampaignId,
                CustomerId = fixtureData.CampaignCompletion.CustomerId,
                IsCompleted = true
            });

            fixtureData.ConditionCompletions.Add(fixtureData.NewConditionCompletion);

            fixtureData.SetupAllMocks();

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            fixtureData.NewConditionCompletion.IsCompleted = false;
            fixtureData.NewConditionCompletion.CurrentCount = 0;

            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            fixtureData.NewConditionCompletion.IsCompleted = false;
            fixtureData.NewConditionCompletion.CurrentCount = 0;

            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(4));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));
        }

        [Fact]
        public async Task ShouldCompleteCampaign_WhenThereIsAnotherCampaignCompleted()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            var campaignId2 = Guid.NewGuid().ToString("D");
            var campaign2 = new CampaignModel
            {
                CompletionCount = 2,
                Id = campaignId2,
                Name = "SignUp2",
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = Guid.NewGuid().ToString("D"),
                        CompletionCount = 3,
                        Id = campaignId2,
                        ImmediateReward = 1,
                        BonusType = fixtureData.BonusType
                    }
                }
            };

            fixtureData.CampaignModels.Add(campaign2);

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                IsCompleted = true,
                CampaignId = campaignId2,
                CustomerId = campaignId2,
                Id = Guid.NewGuid().ToString("D")
            });

            fixtureData.ConditionCompletions.Add(fixtureData.NewConditionCompletion);

            fixtureData.SetupAllMocks();

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                fixtureData.PartnerId,
                fixtureData.LocationId,
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(2));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));
        }

        [Fact]
        public async Task ShouldCompleteCampaign_WhenPartnerIdExistAndAllOtherRequirementsAreFulfilled()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            var partnerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var campaignId2 = Guid.NewGuid().ToString("D");
            var campaign2 = new CampaignModel
            {
                CompletionCount = 2,
                Id = campaignId2,
                Name = "SignUp2",
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = Guid.NewGuid().ToString("D"),
                        CompletionCount = 3,
                        Id = campaignId2,
                        ImmediateReward = 1,
                        BonusType = fixtureData.BonusType,
                        PartnerIds = new Guid[]
                        {
                            partnerId,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                        }
                    }
                }
            };

            fixtureData.CampaignModels.Add(campaign2);

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                IsCompleted = true,
                CampaignId = campaignId2,
                CustomerId = campaignId2,
                Id = Guid.NewGuid().ToString("D")
            });

            fixtureData.ConditionCompletions.Add(fixtureData.NewConditionCompletion);

            fixtureData.SetupAllMocks();

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                partnerId.ToString(""),
                locationId.ToString(""),
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(2));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));
        }
        
        [Fact]
        public async Task ShouldCompleteStakableCampaignsWhereStakingEnabledOnConditions()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            var partnerId = Guid.NewGuid();
            var secondBonusType = new BonusType
            {
                CreationDate = DateTime.UtcNow,
                DisplayName = "secondbonustype",
                IsAvailable = true,
                IsHidden = false,
                IsStakeable = true,
                RewardHasRatio = false,
                Type = "secondbonustype"
            };

            var campaignId2 = Guid.NewGuid().ToString("D");
            var campaign2 = new CampaignModel
            {
                CompletionCount = 1,
                Id = campaignId2,
                Name = "SignUp2",
                
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = campaignId2,
                        CompletionCount = 1,
                        Id = Guid.NewGuid().ToString("D"),
                        ImmediateReward = 1,
                        BonusType = fixtureData.BonusType,
                        HasStaking = true
                    },
                    new Condition
                    {
                        CampaignId = campaignId2,
                        CompletionCount = 1,
                        Id = Guid.NewGuid().ToString("D"),
                        ImmediateReward = 1,
                        BonusType = secondBonusType
                    }
                }
            };

            var campaignId3 = Guid.NewGuid().ToString("D");
            var campaign3 = new CampaignModel
            {
                CompletionCount = 1,
                Id = campaignId3,
                Name = "SignUp3",
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = campaignId3,
                        CompletionCount = 1,
                        Id = Guid.NewGuid().ToString("D"),
                        ImmediateReward = 1,
                        BonusType = fixtureData.BonusType,
                        HasStaking = true
                    },
                    new Condition
                    {
                        CampaignId = campaignId3,
                        CompletionCount = 1,
                        Id = Guid.NewGuid().ToString("D"),
                        ImmediateReward = 1,
                        BonusType = secondBonusType
                    }
                }
            };
            
            fixtureData.CampaignModel.Conditions = new List<Condition>
            {
                fixtureData.ConditionModel,
                new Condition
                {
                    CampaignId = fixtureData.CampaignModel.Id,
                    CompletionCount = 1,
                    Id = Guid.NewGuid().ToString("D"),
                    ImmediateReward = 1,
                    BonusType = secondBonusType
                }
            };
            
            fixtureData.CampaignModels.Add(campaign2);
            fixtureData.CampaignModels.Add(campaign3);
            
            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CustomerId = fixtureData.CustomerId,
                CampaignId = campaignId2,
                CampaignCompletionCount = 0,
                IsCompleted = false,
                Id = Guid.NewGuid().ToString()
            });
            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                CustomerId = fixtureData.CustomerId,
                CampaignId = campaignId3,
                CampaignCompletionCount = 0,
                IsCompleted = false,
                Id = Guid.NewGuid().ToString()
            });

            fixtureData.SetupAllMocks();

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);
            
            var data = new Dictionary<string, string>()
            {
                {"StakedCampaignId", campaignId3}
            };
            
            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                null,
                null,
                data,
                fixtureData.BonusTypeName);
            
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                null,
                null,
                new Dictionary<string, string>(),
                secondBonusType.Type);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ShouldCompleteCampaign_WhenPartnerIdDoesNotExistAndAllOtherRequirementsAreFulfilled()
        {
            // Arrange
            var fixtureData = new CampaignServiceBonusProcessingTestsFixture();

            var partnerId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var campaignId2 = Guid.NewGuid().ToString("D");
            var campaign2 = new CampaignModel
            {
                CompletionCount = 2,
                Id = campaignId2,
                Name = "SignUp2",
                Conditions = new List<Condition>
                {
                    new Condition
                    {
                        CampaignId = Guid.NewGuid().ToString("D"),
                        CompletionCount = 3,
                        Id = campaignId2,
                        ImmediateReward = 1,
                        BonusType = fixtureData.BonusType,
                        PartnerIds = new Guid[]
                        {
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                        }
                    }
                }
            };

            fixtureData.CampaignModels.Add(campaign2);

            fixtureData.CampaignCompletions.Add(new CampaignCompletion
            {
                IsCompleted = true,
                CampaignId = campaignId2,
                CustomerId = campaignId2,
                Id = Guid.NewGuid().ToString("D")
            });

            fixtureData.ConditionCompletions.Add(fixtureData.NewConditionCompletion);

            fixtureData.SetupAllMocks();

            fixtureData.BonusCalculatorServiceMock.Setup(b =>
                    b.CalculateConditionRewardAmountAsync(It.IsAny<Condition>(), It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(10);

            // Execute
            await fixtureData.CampaignServiceInstance.ProcessEventForCustomerAsync(
                fixtureData.CustomerId,
                partnerId.ToString(""),
                locationId.ToString(""),
                fixtureData.EventDataEmpty,
                fixtureData.BonusTypeName);

            // Assert
            fixtureData.BonusOperationServiceMock.VerifyAll();

            fixtureData.BonusOperationServiceMock.Verify(c =>
                c.AddBonusOperationAsync(It.IsAny<BonusOperation>()), Times.Exactly(2));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.CampaignModel.Reward)));

            fixtureData.BonusOperationServiceMock.Verify(x =>
                x.AddBonusOperationAsync(It.Is<BonusOperation>(p => p.Reward == fixtureData.ConditionModel.ImmediateReward)));
        }
    }
}

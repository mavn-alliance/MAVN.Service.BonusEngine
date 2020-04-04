using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Logs;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Services;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Lykke.Service.EligibilityEngine.Client.Models.ConversionRate.Responses;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace MAVN.Service.BonusEngine.DomainServices.Tests
{
    public class BonusCalculatorServiceTests
    {
        private const string AmountName = "Amount";
        private const string BaseCurrencyCode = "CHF";
        private const string TokenName = "TOKEN";

        private readonly Mock<IEligibilityEngineClient> _eligibilityEngineClientMock =
            new Mock<IEligibilityEngineClient>();

        private readonly IBonusCalculatorService _service;

        public BonusCalculatorServiceTests()
        {
            _service = new BonusCalculatorService(
                BaseCurrencyCode,
                TokenName,
                EmptyLogFactory.Instance,
                _eligibilityEngineClientMock.Object);
        }

        [Fact]
        public async Task Calculate_Reward_Using_Conversion_Rate()
        {
            // arrange

            var campaign = new Domain.Models.Campaign
            {
                AmountInTokens = 10,
                AmountInCurrency = 100,
                RewardType = RewardType.ConversionRate,
                Id = Guid.NewGuid().ToString()
            };

            var conditionCompletions = new List<ConditionCompletion>
            {
                new ConditionCompletion
                {
                    Data = new[]
                    {
                        new Dictionary<string, string> {{AmountName, "10"}},
                        new Dictionary<string, string> {{AmountName, "20"}}
                    }
                }
            };

            var customerId = Guid.NewGuid();

            Money18 expectedAmount = 30;

            _eligibilityEngineClientMock.Setup(o => o.ConversionRate.GetAmountByEarnRuleAsync(
                    It.IsAny<ConvertAmountByEarnRuleRequest>()))
                .ReturnsAsync(new ConvertAmountByEarnRuleResponse()
                {
                    Amount = 30
                });

            // act

            Money18 actualAmount = await _service.CalculateRewardAmountAsync(campaign, customerId.ToString(), conditionCompletions);

            // assert

            Assert.Equal(actualAmount, expectedAmount);
        }

        [Fact]
        public async Task Calculate_Reward_Using_One_To_One_Rate_If_Not_Specified()
        {
            // arrange

            var campaign = new Domain.Models.Campaign
            {
                RewardType = RewardType.ConversionRate,
                Id = Guid.NewGuid().ToString()
            };

            var customerId = Guid.NewGuid();

            var conditionCompletions = new List<ConditionCompletion>
            {
                new ConditionCompletion
                {
                    Data = new[]
                    {
                        new Dictionary<string, string> {{AmountName, "10"}},
                        new Dictionary<string, string> {{AmountName, "20"}}
                    }
                }
            };

            _eligibilityEngineClientMock.Setup(o => o.ConversionRate.GetAmountByEarnRuleAsync(
            It.IsAny<ConvertAmountByEarnRuleRequest>()))
                .ReturnsAsync(new ConvertAmountByEarnRuleResponse()
                {
                    Amount = 30
                });


            Money18 expectedAmount = 30;

            // act

            Money18 actualAmount = await _service.CalculateRewardAmountAsync(campaign, customerId.ToString(), conditionCompletions);

            // assert

            Assert.Equal(actualAmount, expectedAmount);
        }

        [Fact]
        public async Task Calculate_Reward_Using_Percentage()
        {
            // arrange
            var campaign = new Domain.Models.Campaign { Reward = 10, RewardType = RewardType.Percentage, Id = Guid.NewGuid().ToString() };

            var customerId = Guid.NewGuid();

            var conditionCompletions = new List<ConditionCompletion>
            {
                new ConditionCompletion
                {
                    Data = new[]
                    {
                        new Dictionary<string, string> {{AmountName, "10.0"}},
                        new Dictionary<string, string> {{AmountName, "20.0"}}
                    }
                }
            };

            Money18 expectedAmount = 3;

            _eligibilityEngineClientMock
                .Setup(o => o.ConversionRate.GetAmountByEarnRuleAsync(It.Is<ConvertAmountByEarnRuleRequest>(request => request.Amount == expectedAmount)))
                .ReturnsAsync(new ConvertAmountByEarnRuleResponse()
                {
                    Amount = 3
                });

            // act
            var actualAmount = await _service.CalculateRewardAmountAsync(campaign, customerId.ToString(), conditionCompletions);

            // assert

            Assert.Equal(actualAmount, expectedAmount);
        }

        [Fact]
        public void Calculate_Ratio_Reward_Give_Reward_Only_For_20_Threshold_When_Already_Given_For_10()
        {
            // Arrange
            var purchaseCompletionPercentage = 40m;

            var paymentId = Guid.NewGuid().ToString();

            var ratioData = new Dictionary<string, string>{
               {"PurchaseCompletionPercentage",purchaseCompletionPercentage.ToString()},
                {"GivenRatioBonusPercent", "10" }
            };

            var conditionCompletionData = new Dictionary<string, string> {
            {
                paymentId, JsonConvert.SerializeObject(ratioData)
            }};

            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1,
                Data = new[] { conditionCompletionData }
            };

            var ratios = new List<RatioAttribute>()
            {
                new RatioAttribute() {Order = 1, PaymentRatio = 10m, RewardRatio = 20m, Threshold = 10m},
                new RatioAttribute() {Order = 2, PaymentRatio = 10m, RewardRatio = 20m, Threshold = 20m},
                new RatioAttribute() {Order = 3, PaymentRatio = 80m, RewardRatio = 70m, Threshold = 100m},
            };

            //Act
            var result = _service.CalculateRatioReward(ratios, conditionCompletionData, 1000);

            //Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void Calculate_Ratio_Reward_Give_Reward_For_Threshold_20_And_For_10_When_No_Reward_Given()
        {
            // Arrange
            var purchaseCompletionPercentage = 40m;
            var paymentId = Guid.NewGuid().ToString();

            var ratioData = new Dictionary<string, string>{
                {"PurchaseCompletionPercentage",purchaseCompletionPercentage.ToString()},
                {"GivenRatioBonusPercent", "0" }
            };

            var conditionCompletionData = new Dictionary<string, string> {
            {
                paymentId, JsonConvert.SerializeObject(ratioData)
            }};

            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1,
                Data = new[] { conditionCompletionData }
            };

            var ratios = new List<RatioAttribute>()
            {
                new RatioAttribute() {Order = 1, PaymentRatio = 10m, RewardRatio = 20m, Threshold = 10m},
                new RatioAttribute() {Order = 2, PaymentRatio = 10m, RewardRatio = 20m, Threshold = 20m},
                new RatioAttribute() {Order = 3, PaymentRatio = 80m, RewardRatio = 70m, Threshold = 100m},
            };

            //Act
            var result = _service.CalculateRatioReward(ratios, conditionCompletionData, 1000);

            //Assert
            Assert.Equal(400, result);
        }
    }
}

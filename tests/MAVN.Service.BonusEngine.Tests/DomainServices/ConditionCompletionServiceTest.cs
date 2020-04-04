using AutoFixture;
using MAVN.Service.BonusEngine.Domain.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.DomainServices;
using Newtonsoft.Json;
using Xunit;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class ConditionCompletionServiceTest
    {
        private const string GivenRatioBonusPercent = "GivenRatioBonusPercent";
        private const string PurchaseCompletionPercentage = "PurchaseCompletionPercentage";

        // InsertAsync
        [Fact]
        public async Task Should_CallRepositoryInsertAsync_When_CallingInsertAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var conditionCompletion = fixture.Create<ConditionCompletion>();
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.InsertAsync(conditionCompletion);

            // Assert
            conditionCompletionRepositoryMock.Verify(x => x.InsertAsync(conditionCompletion));
        }

        // GetConditionCompletionsAsync
        [Fact]
        public async Task Should_CallRepositoryGetConditionCompletionsAsync_When_CallingGetConditionCompletionsAsync()
        {
            // Arrange
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.GetConditionCompletionsAsync();

            // Assert
            conditionCompletionRepositoryMock.Verify(x => x.GetConditionCompletionsAsync());
        }

        // GetConditionCompletion
        [Fact]
        public async Task Should_CallRepositoryGetConditionCompletion_When_CallingGetConditionCompletion()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.GetConditionCompletionAsync(customerId.ToString("D"), conditionId.ToString("D"));

            // Assert
            conditionCompletionRepositoryMock.Verify(x => x.GetConditionCompletion(customerId, conditionId));
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenCallingGetConditionCompletionWithNotGuidCustomerId()
        {
            // Arrange
            var fixture = new Fixture();
            var customerId = fixture.Create<string>().Substring(0, 10);
            var conditionId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetConditionCompletionAsync(customerId, conditionId));
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenCallingGetConditionCompletionWithNotGuidConditionId()
        {
            // Arrange
            var fixture = new Fixture();
            var customerId = Guid.NewGuid().ToString("D");
            var conditionId = fixture.Create<string>().Substring(0, 10);
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetConditionCompletionAsync(customerId, conditionId));
        }

        [Fact]
        public async Task ShouldNotThrowArgumentException_WhenCallingGetConditionCompletionWithWithGuidCustomerIdAndGuidConditionId()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>();
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.GetConditionCompletionAsync(customerId, conditionId);

            // Assert
            // Test will fail on exception
        }

        [Fact]
        public async Task ShouldSetConditionCompleted_WhenValidGuidAsStringIsPassed()
        {
            // Arrange
            var conditionCompletionId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.SetConditionCompletedAsync(Guid.Parse(conditionCompletionId)))
                .Returns(Task.CompletedTask);
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.SetConditionCompletedAsync(conditionCompletionId);

            // Assert
            conditionCompletionRepositoryMock.Verify(c => c.SetConditionCompletedAsync(Guid.Parse(conditionCompletionId)), Times.Once);
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidGuidAsStringIsPassed()
        {
            // Arrange
            var conditionCompletionId = "Not a valid guid";
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.SetConditionCompletedAsync(conditionCompletionId);
            });
        }

        [Fact]
        public async Task ShouldIncreaseCompletionCount_WhenValidGuidInConditionCompletionObjectIsPassed()
        {
            // Arrange
            var conditionCompletionId = Guid.NewGuid().ToString("D");
            var emptyData = new Dictionary<string, string>();
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var conditionCompletion = new ConditionCompletion
            {
                Id = conditionCompletionId
            };

            conditionCompletionRepositoryMock
                .Setup(c => c.IncreaseCompletionCountAsync(Guid.Parse(conditionCompletionId), emptyData, 1))
                .Returns(Task.CompletedTask);

            conditionCompletionRepositoryMock
                .Setup(c => c.GetConditionCompletion(Guid.Parse(conditionCompletionId)))
                .ReturnsAsync(conditionCompletion);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.IncreaseCompletionCountAsync(conditionCompletion, emptyData, 1);

            // Assert
            conditionCompletionRepositoryMock.Verify(c => c.IncreaseCompletionCountAsync(Guid.Parse(conditionCompletionId), emptyData, 1), Times.Once);
            conditionCompletionRepositoryMock.Verify(c => c.GetConditionCompletion(Guid.Parse(conditionCompletionId)), Times.Once);
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidGuidInConditionCompletionObjectIsPassed()
        {
            // Arrange
            var conditionCompletionId = "Not a valid guid";
            var emptyData = new Dictionary<string, string>();
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var conditionCompletion = new ConditionCompletion
            {
                Id = conditionCompletionId
            };

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.IncreaseCompletionCountAsync(conditionCompletion, emptyData, 1);
            });
        }

        [Fact]
        public async Task ShouldGetConditionCompletion_WhenValidCustomerAndConditionAsStringArePassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.GetConditionCompletionsAsync(Guid.Parse(customerId), Guid.Parse(conditionId)))
                .ReturnsAsync(new List<ConditionCompletion>().AsReadOnly());
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.GetConditionCompletionsAsync(customerId, conditionId);

            // Assert
            conditionCompletionRepositoryMock.Verify(c => c.GetConditionCompletionsAsync(Guid.Parse(customerId), Guid.Parse(conditionId)), Times.Once);
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidCustomerIdAsStringIsPassed()
        {
            // Arrange
            var customerId = "Not a valid guid";
            var conditionId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.GetConditionCompletionsAsync(customerId, conditionId);
            });
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidConditionIdAsStringIsPassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var conditionId = "Not a valid guid";
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.GetConditionCompletionsAsync(customerId, conditionId);
            });
        }

        [Fact]
        public async Task ShouldGetConditionCompletion_WhenValidCustomerConditionAndCampaignAsStringArePassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.GetConditionCompletionsAsync(Guid.Parse(customerId), Guid.Parse(campaignId)))
                .ReturnsAsync(new List<ConditionCompletion>().AsReadOnly());
            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.GetConditionCompletionsAsync(customerId, campaignId);

            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.GetConditionCompletionsAsync(Guid.Parse(customerId), Guid.Parse(campaignId)), Times.Once);
        }

        [Fact]
        public async Task ShouldInsertConditionCompletion_WhenPassedConditionCompletionIsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var data = new Dictionary<string, string>
            {
                { "key", "value" }
            };

            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.InsertAsync(It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(Guid.NewGuid());

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.IncreaseOrCreateAsync(
                customerId,
                null,
                data,
                new Condition { CampaignId = campaignId, Id = conditionId.ToString() });

            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.InsertAsync(It.Is<ConditionCompletion>(x =>
                    x.IsCompleted == false &&
                    x.CampaignId == campaignId &&
                    x.CurrentCount == 1 &&
                    x.ConditionId == conditionId)), Times.Once);
        }

        [Fact]
        public async Task ShouldIncreaseCompletionCount_WhenPassedConditionCompletionIsNotCompletedYet()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var data = new Dictionary<string, string>
            {
                { "key", "value" }
            };
            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1
            };

            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.IncreaseCompletionCountAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            conditionCompletionRepositoryMock
                .Setup(c => c.GetConditionCompletion(It.IsAny<Guid>()))
                .ReturnsAsync(conditionCompletion);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.IncreaseOrCreateAsync(
                customerId,
                conditionCompletion,
                data,
                new Condition { CampaignId = campaignId, Id = conditionId.ToString(), CompletionCount = 2 });

            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Once);
            conditionCompletionRepositoryMock.Verify(c =>
                c.GetConditionCompletion(It.IsAny<Guid>()), Times.Once);
            conditionCompletionRepositoryMock.Verify(c =>
                c.InsertAsync(It.IsAny<ConditionCompletion>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCompleteConditionCompletion_WhenCompletedConditionCompletionIsPassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var data = new Dictionary<string, string>
            {
                { "key", "value" }
            };
            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1
            };

            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.SetConditionCompletedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            await service.IncreaseOrCreateAsync(
                customerId,
                conditionCompletion,
                data,
                new Condition { CampaignId = campaignId, Id = conditionId.ToString(), CompletionCount = 1 });

            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.GetConditionCompletion(It.IsAny<Guid>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.InsertAsync(It.IsAny<ConditionCompletion>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidCustomerIdAsStringIsPassedButConditionAndCampaignAreValid()
        {
            // Arrange
            var customerId = "Not a valid guid";
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.GetConditionCompletionsAsync(customerId, campaignId);
            });
        }

        [Fact]
        public async Task ShouldThrowArgumentException_WhenInvalidCampaignIdAsStringIsPassedButCustomerAndConditionAreValid()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = "Not a valid guid";
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.GetConditionCompletionsAsync(customerId, campaignId);
            });
        }

        [Fact]
        public async Task ShouldNotCompleteConditionCompletion_WhenConditionRewardRatioIsPassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var paymentId = Guid.NewGuid().ToString();

            var ratioData = new Dictionary<string, string>{
                {PurchaseCompletionPercentage,"20"},
                {"GivenRatioBonusPercent", "10" }
            };

            var conditionCompletionData = new Dictionary<string, string> {
            {
                paymentId, JsonConvert.SerializeObject(ratioData)
            }};

            var newData = new Dictionary<string, string>
            {
                {PurchaseCompletionPercentage, "40" },
                {"PaymentId", paymentId }
            };

            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1,
                Data = new[] { conditionCompletionData }
            };

            var condition = new Condition
            {
                CampaignId = campaignId,
                Id = conditionId,
                CompletionCount = 1,
                RewardHasRatio = true,
                RewardRatio = new RewardRatioAttribute()
                {
                    Ratios = new List<RatioAttribute>()
                    {
                        new RatioAttribute()
                        {
                            Order = 1,
                            RewardRatio = 20m,
                            PaymentRatio = 10m,
                            Threshold = 10m
                        },
                        new RatioAttribute()
                        {
                            Order = 2,
                            PaymentRatio = 10m,
                            RewardRatio = 20m,
                            Threshold = 20m
                        },
                        new RatioAttribute()
                        {
                            Order = 3,
                            PaymentRatio = 70m,
                            RewardRatio = 70m,
                            Threshold = 100m
                        },
                    }
                }
            };
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.SetConditionCompletedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            conditionCompletionRepositoryMock
                .Setup(c => c.UpdateAsync(It.IsAny<ConditionCompletion>()))
                .Returns(Task.CompletedTask);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            var conditionUpdated = await service.IncreaseOrCreateAsync(
                customerId,
                conditionCompletion,
                newData,
                condition);
            var updatedData = conditionUpdated.Data.FirstOrDefault();

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedData.Values.FirstOrDefault());
            
            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.GetConditionCompletion(It.IsAny<Guid>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<Guid>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.UpdateAsync(It.IsAny<ConditionCompletion>()), Times.Once);

            Assert.NotNull(updatedData);
            Assert.Equal("40", result[PurchaseCompletionPercentage]);
        }

        [Fact]
        public async Task ShouldNotCompleteConditionCompletion_WhenConditionCompletionNullRewardRatioIsPassed()
        {
            // Arrange
            var customerId = Guid.NewGuid().ToString("D");
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var paymentId = Guid.NewGuid().ToString();

            var newData = new Dictionary<string, string>
            {
                { PurchaseCompletionPercentage, "40" },
                {"PaymentId", paymentId }
            };

            var condition = new Condition
            {
                CampaignId = campaignId,
                Id = conditionId,
                CompletionCount = 1,
                RewardHasRatio = true,
                RewardRatio = new RewardRatioAttribute()
                {
                    Ratios = new List<RatioAttribute>()
                    {
                        new RatioAttribute()
                        {
                            Order = 1,
                            RewardRatio = 20m,
                            PaymentRatio = 10m,
                            Threshold = 10m
                        },
                        new RatioAttribute()
                        {
                            Order = 2,
                            PaymentRatio = 10m,
                            RewardRatio = 20m,
                            Threshold = 20m
                        },
                        new RatioAttribute()
                        {
                            Order = 3,
                            PaymentRatio = 70m,
                            RewardRatio = 70m,
                            Threshold = 100m
                        },
                    }
                }
            };
            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            conditionCompletionRepositoryMock
                .Setup(c => c.InsertAsync(It.IsAny<ConditionCompletion>()))
                .ReturnsAsync(Guid.NewGuid());

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            var conditionUpdated = await service.IncreaseOrCreateAsync(
                customerId,
                null,
                newData,
                condition);

            var updatedData = conditionUpdated.Data.FirstOrDefault();

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedData.Values.FirstOrDefault());

            // Assert
            conditionCompletionRepositoryMock.Verify(c =>
                c.IncreaseCompletionCountAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.GetConditionCompletion(It.IsAny<Guid>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.SetConditionCompletedAsync(It.IsAny<Guid>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.UpdateAsync(It.IsAny<ConditionCompletion>()), Times.Never);
            conditionCompletionRepositoryMock.Verify(c =>
                c.InsertAsync(It.IsAny<ConditionCompletion>()), Times.Once);

            Assert.NotNull(updatedData);
            Assert.Equal("40", result[PurchaseCompletionPercentage]);
            Assert.Equal("0", result[GivenRatioBonusPercent]);
        }

        [Fact]
        public void ShouldSrtLastGivenThresholdBonus_WhenSetConditionCompletionLastGivenRatioRewardIsCalled()
        {
            // Arrange
            var campaignId = Guid.NewGuid().ToString("D");
            var conditionId = Guid.NewGuid().ToString("D");
            var paymentId = Guid.NewGuid().ToString();

            var ratioData = new Dictionary<string, string>{
                {"PurchaseCompletionPercentage","20"},
                {"GivenRatioBonusPercent", "10" }
            };

            var conditionCompletionData = new Dictionary<string, string> {
            {
                paymentId, JsonConvert.SerializeObject(ratioData)
            }};

            var newData = new Dictionary<string, string>
            {
                {PurchaseCompletionPercentage, "40" },
                {"PaymentId", paymentId }
            };

            var conditionCompletion = new ConditionCompletion
            {
                Id = Guid.NewGuid().ToString(),
                CurrentCount = 1,
                Data = new[] { conditionCompletionData }
            };

            var condition = new Condition
            {
                CampaignId = campaignId,
                Id = conditionId,
                CompletionCount = 1,
                RewardHasRatio = true,
                RewardRatio = new RewardRatioAttribute()
                {
                    Ratios = new List<RatioAttribute>()
                    {
                        new RatioAttribute()
                        {
                            Order = 1,
                            PaymentRatio = 10m,
                            RewardRatio = 20m,
                            Threshold = 10m
                        },
                        new RatioAttribute()
                        {
                            Order = 2,
                            PaymentRatio = 10m,
                            RewardRatio = 20m,
                            Threshold = 20m
                        },
                        new RatioAttribute()
                        {
                            Order = 3,
                            PaymentRatio = 80m,
                            RewardRatio = 70m,
                            Threshold = 100m
                        },
                    }
                }
            };

            var conditionCompletionRepositoryMock = new Mock<IConditionCompletionRepository>(MockBehavior.Strict);

            var service = new ConditionCompletionService(conditionCompletionRepositoryMock.Object);

            // Act
            var threshold = service.SetConditionCompletionLastGivenRatioReward(newData, condition, conditionCompletion, out bool test);

            var updatedData = conditionCompletion.Data.FirstOrDefault(c=>c.ContainsKey(paymentId));

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedData.Values.FirstOrDefault());

            Assert.Equal(20, threshold);
            Assert.NotNull(updatedData);
            Assert.Single(updatedData);

            Assert.Equal(2, result.Count);
            Assert.Equal("20", result[GivenRatioBonusPercent]);
        }
    }
}

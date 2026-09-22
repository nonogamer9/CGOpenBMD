using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Purchasing.Security;

namespace UnityEngine.Purchasing
{
	public class SubscriptionInfo
	{
		private Result is_subscribed;

		private Result is_expired;

		private Result is_cancelled;

		private Result is_free_trial;

		private Result is_auto_renewing;

		private Result is_introductory_price_period;

		private string productId;

		private DateTime purchaseDate;

		private DateTime subscriptionExpireDate;

		private DateTime subscriptionCancelDate;

		private TimeSpan remainedTime;

		private string introductory_price;

		private TimeSpan introductory_price_period;

		private long introductory_price_cycles;

		private TimeSpan freeTrialPeriod;

		private TimeSpan subscriptionPeriod;

		private string free_trial_period_string;

		private string sku_details;

		public SubscriptionInfo(AppleInAppPurchaseReceipt r, string intro_json)
		{
			AppleStoreProductType appleStoreProductType = (AppleStoreProductType)Enum.Parse(typeof(AppleStoreProductType), r.productType.ToString());
			if (appleStoreProductType == AppleStoreProductType.Consumable || appleStoreProductType == AppleStoreProductType.NonConsumable)
			{
				throw new InvalidProductTypeException();
			}
			if (!string.IsNullOrEmpty(intro_json))
			{
				Dictionary<string, object> dic = (Dictionary<string, object>)MiniJson.JsonDecode(intro_json);
				int num = -1;
				SubscriptionPeriodUnit subscriptionPeriodUnit = SubscriptionPeriodUnit.NotAvailable;
				introductory_price = dic.TryGetString("introductoryPrice") + dic.TryGetString("introductoryPriceLocale");
				if (string.IsNullOrEmpty(introductory_price))
				{
					introductory_price = "not available";
				}
				else
				{
					try
					{
						introductory_price_cycles = Convert.ToInt64(dic.TryGetString("introductoryPriceNumberOfPeriods"));
						num = Convert.ToInt32(dic.TryGetString("numberOfUnits"));
						subscriptionPeriodUnit = (SubscriptionPeriodUnit)Convert.ToInt32(dic.TryGetString("unit"));
					}
					catch (Exception message)
					{
						Debug.unityLogger.Log("Unable to parse introductory period cycles and duration, this product does not have configuration of introductory price period", message);
						subscriptionPeriodUnit = SubscriptionPeriodUnit.NotAvailable;
					}
				}
				DateTime now = DateTime.Now;
				switch (subscriptionPeriodUnit)
				{
				case SubscriptionPeriodUnit.Day:
					introductory_price_period = TimeSpan.FromTicks(TimeSpan.FromDays(1.0).Ticks * num);
					break;
				case SubscriptionPeriodUnit.Month:
					introductory_price_period = TimeSpan.FromTicks((now.AddMonths(1) - now).Ticks * num);
					break;
				case SubscriptionPeriodUnit.Week:
					introductory_price_period = TimeSpan.FromTicks(TimeSpan.FromDays(7.0).Ticks * num);
					break;
				case SubscriptionPeriodUnit.Year:
					introductory_price_period = TimeSpan.FromTicks((now.AddYears(1) - now).Ticks * num);
					break;
				case SubscriptionPeriodUnit.NotAvailable:
					introductory_price_period = TimeSpan.Zero;
					introductory_price_cycles = 0L;
					break;
				}
			}
			else
			{
				introductory_price = "not available";
				introductory_price_period = TimeSpan.Zero;
				introductory_price_cycles = 0L;
			}
			DateTime utcNow = DateTime.UtcNow;
			purchaseDate = r.purchaseDate;
			productId = r.productID;
			subscriptionExpireDate = r.subscriptionExpirationDate;
			subscriptionCancelDate = r.cancellationDate;
			if (appleStoreProductType == AppleStoreProductType.NonRenewingSubscription)
			{
				is_subscribed = Result.Unsupported;
				is_expired = Result.Unsupported;
				is_cancelled = Result.Unsupported;
				is_free_trial = Result.Unsupported;
				is_auto_renewing = Result.Unsupported;
				is_introductory_price_period = Result.Unsupported;
			}
			else
			{
				is_cancelled = ((r.cancellationDate.Ticks <= 0 || r.cancellationDate.Ticks >= utcNow.Ticks) ? Result.False : Result.True);
				is_subscribed = ((r.subscriptionExpirationDate.Ticks < utcNow.Ticks) ? Result.False : Result.True);
				is_expired = ((r.subscriptionExpirationDate.Ticks <= 0 || r.subscriptionExpirationDate.Ticks >= utcNow.Ticks) ? Result.False : Result.True);
				is_free_trial = ((r.isFreeTrial != 1) ? Result.False : Result.True);
				is_auto_renewing = ((appleStoreProductType != AppleStoreProductType.AutoRenewingSubscription || is_cancelled != Result.False || is_expired != Result.False) ? Result.False : Result.True);
				is_introductory_price_period = ((r.isIntroductoryPricePeriod != 1) ? Result.False : Result.True);
			}
			if (is_subscribed == Result.True)
			{
				remainedTime = r.subscriptionExpirationDate.Subtract(utcNow);
			}
			else
			{
				remainedTime = TimeSpan.Zero;
			}
		}

		public SubscriptionInfo(string skuDetails, bool isAutoRenewing, DateTime purchaseDate, bool isFreeTrial, bool hasIntroductoryPriceTrial, bool purchaseHistorySupported, string updateMetadata)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(skuDetails);
			if ((string)dictionary["type"] == "inapp")
			{
				throw new InvalidProductTypeException();
			}
			productId = (string)dictionary["productId"];
			this.purchaseDate = purchaseDate;
			is_subscribed = Result.True;
			is_auto_renewing = ((!isAutoRenewing) ? Result.False : Result.True);
			is_expired = Result.False;
			is_cancelled = (isAutoRenewing ? Result.False : Result.True);
			is_free_trial = Result.False;
			string text = null;
			if (dictionary.ContainsKey("subscriptionPeriod"))
			{
				text = (string)dictionary["subscriptionPeriod"];
			}
			string period_string = null;
			if (dictionary.ContainsKey("freeTrialPeriod"))
			{
				period_string = (string)dictionary["freeTrialPeriod"];
			}
			string text2 = null;
			if (dictionary.ContainsKey("introductoryPrice"))
			{
				text2 = (string)dictionary["introductoryPrice"];
			}
			string text3 = null;
			if (dictionary.ContainsKey("introductoryPricePeriod"))
			{
				text3 = (string)dictionary["introductoryPricePeriod"];
			}
			long num = 0L;
			if (dictionary.ContainsKey("introductoryPriceCycles"))
			{
				num = (long)dictionary["introductoryPriceCycles"];
			}
			free_trial_period_string = period_string;
			subscriptionPeriod = computePeriodTimeSpan(parsePeriodTimeSpanUnits(text));
			freeTrialPeriod = TimeSpan.Zero;
			if (isFreeTrial)
			{
				freeTrialPeriod = parseTimeSpan(period_string);
			}
			introductory_price = text2;
			introductory_price_cycles = num;
			introductory_price_period = TimeSpan.Zero;
			is_introductory_price_period = Result.False;
			TimeSpan ts = TimeSpan.Zero;
			if (hasIntroductoryPriceTrial)
			{
				if (text3 != null && text3.Equals(text))
				{
					introductory_price_period = subscriptionPeriod;
				}
				else
				{
					introductory_price_period = parseTimeSpan(text3);
				}
				ts = accumulateIntroductoryDuration(parsePeriodTimeSpanUnits(text3), introductory_price_cycles);
			}
			TimeSpan timeSpan = TimeSpan.FromSeconds((updateMetadata == null) ? 0.0 : computeExtraTime(updateMetadata, subscriptionPeriod.TotalSeconds));
			TimeSpan timeSpan2 = DateTime.UtcNow.Subtract(purchaseDate);
			if (timeSpan2 <= timeSpan)
			{
				subscriptionExpireDate = purchaseDate.Add(timeSpan);
			}
			else if (timeSpan2 <= freeTrialPeriod.Add(timeSpan))
			{
				is_free_trial = Result.True;
				subscriptionExpireDate = purchaseDate.Add(freeTrialPeriod.Add(timeSpan));
			}
			else if (timeSpan2 < freeTrialPeriod.Add(timeSpan).Add(ts))
			{
				is_introductory_price_period = Result.True;
				DateTime billing_begin_date = this.purchaseDate.Add(freeTrialPeriod.Add(timeSpan));
				subscriptionExpireDate = nextBillingDate(billing_begin_date, parsePeriodTimeSpanUnits(text3));
			}
			else
			{
				DateTime billing_begin_date2 = this.purchaseDate.Add(freeTrialPeriod.Add(timeSpan).Add(ts));
				subscriptionExpireDate = nextBillingDate(billing_begin_date2, parsePeriodTimeSpanUnits(text));
			}
			remainedTime = subscriptionExpireDate.Subtract(DateTime.UtcNow);
			sku_details = skuDetails;
			if (!purchaseHistorySupported)
			{
				is_free_trial = Result.Unsupported;
				subscriptionExpireDate = DateTime.MaxValue;
				remainedTime = TimeSpan.MaxValue;
				is_introductory_price_period = Result.Unsupported;
			}
		}

		public SubscriptionInfo(string productId)
		{
			this.productId = productId;
			is_subscribed = Result.True;
			is_expired = Result.False;
			is_cancelled = Result.Unsupported;
			is_free_trial = Result.Unsupported;
			is_auto_renewing = Result.Unsupported;
			remainedTime = TimeSpan.MaxValue;
			is_introductory_price_period = Result.Unsupported;
			introductory_price_period = TimeSpan.MaxValue;
			introductory_price = null;
			introductory_price_cycles = 0L;
		}

		public string getProductId()
		{
			return productId;
		}

		public DateTime getPurchaseDate()
		{
			return purchaseDate;
		}

		public Result isSubscribed()
		{
			return is_subscribed;
		}

		public Result isExpired()
		{
			return is_expired;
		}

		public Result isCancelled()
		{
			return is_cancelled;
		}

		public Result isFreeTrial()
		{
			return is_free_trial;
		}

		public Result isAutoRenewing()
		{
			return is_auto_renewing;
		}

		public TimeSpan getRemainingTime()
		{
			return remainedTime;
		}

		public Result isIntroductoryPricePeriod()
		{
			return is_introductory_price_period;
		}

		public TimeSpan getIntroductoryPricePeriod()
		{
			return introductory_price_period;
		}

		public string getIntroductoryPrice()
		{
			return string.IsNullOrEmpty(introductory_price) ? "not available" : introductory_price;
		}

		public long getIntroductoryPricePeriodCycles()
		{
			return introductory_price_cycles;
		}

		public DateTime getExpireDate()
		{
			return subscriptionExpireDate;
		}

		public DateTime getCancelDate()
		{
			return subscriptionCancelDate;
		}

		public TimeSpan getFreeTrialPeriod()
		{
			return freeTrialPeriod;
		}

		public TimeSpan getSubscriptionPeriod()
		{
			return subscriptionPeriod;
		}

		public string getFreeTrialPeriodString()
		{
			return free_trial_period_string;
		}

		public string getSkuDetails()
		{
			return sku_details;
		}

		public string getSubscriptionInfoJsonString()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("productId", productId);
			dictionary.Add("is_free_trial", is_free_trial);
			dictionary.Add("is_introductory_price_period", is_introductory_price_period == Result.True);
			dictionary.Add("remaining_time_in_seconds", remainedTime.TotalSeconds);
			return MiniJson.JsonEncode(dictionary);
		}

		private DateTime nextBillingDate(DateTime billing_begin_date, TimeSpanUnits units)
		{
			DateTime dateTime = billing_begin_date;
			while (DateTime.Compare(dateTime, DateTime.UtcNow) <= 0)
			{
				dateTime = dateTime.AddDays(units.days).AddMonths(units.months).AddYears(units.years);
			}
			return dateTime;
		}

		private TimeSpan accumulateIntroductoryDuration(TimeSpanUnits units, long cycles)
		{
			TimeSpan result = TimeSpan.Zero;
			for (long num = 0L; num < cycles; num++)
			{
				result = result.Add(computePeriodTimeSpan(units));
			}
			return result;
		}

		private TimeSpan computePeriodTimeSpan(TimeSpanUnits units)
		{
			DateTime now = DateTime.Now;
			return now.AddDays(units.days).AddMonths(units.months).AddYears(units.years)
				.Subtract(now);
		}

		private double computeExtraTime(string metadata, double new_sku_period_in_seconds)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(metadata);
			long num = (long)dictionary["old_sku_remaining_seconds"];
			long num2 = (long)dictionary["old_sku_price_in_micros"];
			double totalSeconds = parseTimeSpan((string)dictionary["old_sku_period_string"]).TotalSeconds;
			long num3 = (long)dictionary["new_sku_price_in_micros"];
			return (double)num / totalSeconds * (double)num2 / (double)num3 * new_sku_period_in_seconds;
		}

		private TimeSpan parseTimeSpan(string period_string)
		{
			TimeSpan zero = TimeSpan.Zero;
			try
			{
				zero = XmlConvert.ToTimeSpan(period_string);
			}
			catch (Exception)
			{
				zero = ((period_string != null && period_string.Length != 0) ? new TimeSpan(7, 0, 0, 0) : TimeSpan.Zero);
			}
			return zero;
		}

		private TimeSpanUnits parsePeriodTimeSpanUnits(string time_span)
		{
			switch (time_span)
			{
			case "P1W":
				return new TimeSpanUnits(7.0, 0, 0);
			case "P1M":
				return new TimeSpanUnits(0.0, 1, 0);
			case "P3M":
				return new TimeSpanUnits(0.0, 3, 0);
			case "P6M":
				return new TimeSpanUnits(0.0, 6, 0);
			case "P1Y":
				return new TimeSpanUnits(0.0, 0, 1);
			default:
				return new TimeSpanUnits(parseTimeSpan(time_span).Days, 0, 0);
			}
		}
	}
}

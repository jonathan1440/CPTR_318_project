using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;

public class DateObj : MonoBehaviour
{
	public class DateObject
	{
		//returns length of specified month in specified year
		private int month_len(int mon, int yr)
		{
			if (mon == 2)
				return yr % 4 == 0 ? 29 : 28;
			
			if (mon == 4 || mon == 6 || mon == 9 || mon == 11)
				return 30;
			
			if (mon >= 0 || mon < 12)
				return 31;

			return 0;
		}
	
		private readonly string[] days = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
		private readonly string[] day_abrvs = {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		private readonly string[] months =
		{
			"January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
			"November", "December"
		};
		private readonly string[] month_abrvs =
			{"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

		//return date in days since 1/1/1 AD
		private int calc_total_days(int year, int month, int day)
		{
			int total_days = year * 365 + year / 4 + day;

			for (int i = 1; i <= month; i++)
			{
				total_days += month_len(month, year);
			}
		
			return total_days;
		}
		
		public readonly int year;
		public readonly int month;
		public readonly int day;

		public int total_days()
		{
			return calc_total_days(year, month, day);
		}
		
		public int day_of_the_week()
		{
			//total_days is # of days from 1/1/1 AD, and 1/1/1 was a Saturday, so...
			int d = (total_days() + 1) % 7;
			return d == 0 ? 7 : d;
		}

		public DateObject week_start()
		{
			return new DateObject(year, month, day + 1 - day_of_the_week());
		}
	
		public DateObject(int year, int month, int day)
		{
			//apparently it is only necessary to use this. when you have a parameter of the same name
			this.year = year;
			this.month = month;
			this.day = day;

			//get down to numbers within a usable range
			while (this.month > 12 || this.day > month_len(this.month, this.year))
			{
				while (this.month > 12)
				{
					this.year++;
					this.month -= 12;
				}

				while (this.day > month_len(this.month, this.year))
				{
					this.day -= month_len(this.month % 12, this.year);
					this.month++;
				}
			}

			//get up to numbers within a usable range
			while (this.month < 0 || this.day < 0)
			{
				while (this.month < 0)
				{
					this.year--;
					this.month += 12;
				}

				while (this.day < 0)
				{
					this.month--;
					this.day += month_len(this.month % 12, this.year);
				}
			}
		}

		//returns different date formats, helper for ToString overload
		private string string_formatting(string inst)
		{
			switch (inst)
			{
				case "d":
					return day.ToString();
					
				case "D":
					return days[day_of_the_week() - 1];
					
				case "dd":
					return day_abrvs[day_of_the_week() - 1];
				
				case "m":
					return "/" + month.ToString();
				
				case "M":
					return " " + months[month - 1];
				
				case "mm":
					return " " + month_abrvs[month - 1];
				
				case "yy":
				case "Y":
					return ", " + year.ToString();
				case "y":
					return " " + year.ToString();
				
				default:
					return "";
			}
		}
		
		public string ToString(string format)
		{
			string[] ff = format.Split(':');
			string rf = "";

			foreach(string segment in ff)
			{
				rf += string_formatting(segment);
			}
			
			return rf;
		}
		
		public static DateObject operator + (DateObject A, DateObject B)
		{
			return new DateObject(A.year + B.year, A.month + B.month, A.day + B.day);
		}
		
		public static DateObject operator - (DateObject A, DateObject B)
		{
			return new DateObject(Math.Abs(A.year - B.year), A.month - B.month, A.day - B.day);
		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

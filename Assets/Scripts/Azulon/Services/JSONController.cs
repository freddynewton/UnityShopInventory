using System;
using UnityEngine;

namespace Azulon.Services
{
	public static class JSONController
	{
		private const string SAVE_KEY_PREFIX = "Azulon_";

		/// <summary>
		/// Saves data to PlayerPrefs as JSON
		/// </summary>
		/// <typeparam name="T">Type of data to save</typeparam>
		/// <param name="data">Data object to save</param>
		/// <param name="key">Key to save under</param>
		/// <returns>True if save was successful</returns>
		public static bool SaveData<T>(T data, string key)
		{
			try
			{
				if (data == null)
				{
					Debug.LogError($"JSONController: Cannot save null data for key '{key}'");
					return false;
				}

				string json = JsonUtility.ToJson(data, true);
				string fullKey = SAVE_KEY_PREFIX + key;

				PlayerPrefs.SetString(fullKey, json);
				PlayerPrefs.Save();

				Debug.Log($"JSONController: Successfully saved data for key '{key}'");
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to save data for key '{key}': {e.Message}");
				return false;
			}
		}

		/// <summary>
		/// Loads data from PlayerPrefs as JSON
		/// </summary>
		/// <typeparam name="T">Type of data to load</typeparam>
		/// <param name="key">Key to load from</param>
		/// <param name="defaultValue">Default value if load fails or key doesn't exist</param>
		/// <returns>Loaded data or default value</returns>
		public static T LoadData<T>(string key, T defaultValue = default(T))
		{
			try
			{
				string fullKey = SAVE_KEY_PREFIX + key;

				if (!PlayerPrefs.HasKey(fullKey))
				{
					Debug.Log($"JSONController: No save data found for key '{key}', using default value");
					return defaultValue;
				}

				string json = PlayerPrefs.GetString(fullKey);

				if (string.IsNullOrEmpty(json))
				{
					Debug.LogWarning($"JSONController: Empty JSON data for key '{key}', using default value");
					return defaultValue;
				}

				T data = JsonUtility.FromJson<T>(json);
				Debug.Log($"JSONController: Successfully loaded data for key '{key}'");
				return data;
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to load data for key '{key}': {e.Message}");
				return defaultValue;
			}
		}

		/// <summary>
		/// Checks if save data exists for a given key
		/// </summary>
		/// <param name="key">Key to check</param>
		/// <returns>True if data exists</returns>
		public static bool HasData(string key)
		{
			string fullKey = SAVE_KEY_PREFIX + key;
			return PlayerPrefs.HasKey(fullKey);
		}

		/// <summary>
		/// Deletes save data for a given key
		/// </summary>
		/// <param name="key">Key to delete</param>
		/// <returns>True if deletion was successful</returns>
		public static bool DeleteData(string key)
		{
			try
			{
				string fullKey = SAVE_KEY_PREFIX + key;

				if (PlayerPrefs.HasKey(fullKey))
				{
					PlayerPrefs.DeleteKey(fullKey);
					PlayerPrefs.Save();
					Debug.Log($"JSONController: Successfully deleted data for key '{key}'");
					return true;
				}
				else
				{
					Debug.LogWarning($"JSONController: No data found to delete for key '{key}'");
					return false;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to delete data for key '{key}': {e.Message}");
				return false;
			}
		}

		/// <summary>
		/// Deletes all save data with the Azulon prefix
		/// </summary>
		/// <returns>True if deletion was successful</returns>
		public static bool DeleteAllData()
		{
			try
			{
				// Note: Unity's PlayerPrefs doesn't have a built-in way to list all keys
				// So we'll delete known keys. In a production system, you might want to 
				// implement a more sophisticated approach

				string[] knownKeys = { "ItemServiceData" };

				foreach (string key in knownKeys)
				{
					DeleteData(key);
				}

				Debug.Log("JSONController: Successfully deleted all known Azulon data");
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to delete all data: {e.Message}");
				return false;
			}
		}

		/// <summary>
		/// Validates if a JSON string can be deserialized to the given type
		/// </summary>
		/// <typeparam name="T">Type to validate against</typeparam>
		/// <param name="json">JSON string to validate</param>
		/// <returns>True if valid</returns>
		public static bool ValidateJSON<T>(string json)
		{
			try
			{
				if (string.IsNullOrEmpty(json))
					return false;

				JsonUtility.FromJson<T>(json);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Converts an object to JSON string without saving
		/// </summary>
		/// <typeparam name="T">Type of object</typeparam>
		/// <param name="data">Object to convert</param>
		/// <param name="prettyPrint">Whether to format the JSON for readability</param>
		/// <returns>JSON string or null if conversion failed</returns>
		public static string ToJSON<T>(T data, bool prettyPrint = false)
		{
			try
			{
				return JsonUtility.ToJson(data, prettyPrint);
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to convert to JSON: {e.Message}");
				return null;
			}
		}

		/// <summary>
		/// Converts a JSON string to an object without loading from PlayerPrefs
		/// </summary>
		/// <typeparam name="T">Type to convert to</typeparam>
		/// <param name="json">JSON string</param>
		/// <returns>Converted object or default value if conversion failed</returns>
		public static T FromJSON<T>(string json)
		{
			try
			{
				if (string.IsNullOrEmpty(json))
					return default(T);

				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception e)
			{
				Debug.LogError($"JSONController: Failed to convert from JSON: {e.Message}");
				return default(T);
			}
		}
	}
}

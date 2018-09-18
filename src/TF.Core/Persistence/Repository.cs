﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using TF.Core.Entities;

namespace TF.Core.Persistence
{
    public partial class Repository
    {
        private SQLiteConnection _dbConnection;
        private SQLiteTransaction _dbTransaction;

        protected Repository(SQLiteConnection connection)
        {
            _dbConnection = connection;
        }

        public static Repository Create(string path)
        {
            SQLiteConnection.CreateFile(path);

            var dbConnection = new SQLiteConnection($"Data Source={path};Version=3;");
            dbConnection.Open();

            var repository = new Repository(dbConnection);
            repository.CreateTables();
            
            return repository;
        }

        private void CreateTables()
        {
            var sql = "CREATE TABLE PROJECT_CONFIG(KEY TEXT PRIMARY KEY, VAL TEXT)";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE TRANSLATION(ID INTEGER PRIMARY KEY, VISIBLE INTEGER, FIELD TEXT, OFFSET INTEGER, ORIGINAL TEXT, TRANSLATION TEXT)";
            command = new SQLiteCommand(sql, _dbConnection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE OUTPUT_REPLACEMENTS(STRING1 TEXT, STRING2 TEXT)";
            command = new SQLiteCommand(sql, _dbConnection);
            command.ExecuteNonQuery();
        }

        public static Repository Open(string path)
        {
            var dbConnection = new SQLiteConnection($"Data Source={path};Version=3;");
            dbConnection.Open();

            var testDatabase = dbConnection.BeginTransaction(); // Si no es una base de datos sqlite, dará una excepcion
            testDatabase.Rollback();

            var repo = new Repository(dbConnection);
            return repo;
        }

        public void Close()
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public partial class Repository
    {
        public void InsertReplacement(string str1, string str2)
        {
            var sql = "INSERT INTO OUTPUT_REPLACEMENTS(STRING1, STRING2) VALUES (@str1, @str2)";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@str1", str1);
            command.Parameters.AddWithValue("@str2", str2);
            command.ExecuteNonQuery();
        }

        public void InsertReplacements(IList<Tuple<string, string>> replacements)
        {
            _dbTransaction = _dbConnection.BeginTransaction();
            try
            {
                foreach (var str in replacements)
                {
                    InsertReplacement(str.Item1, str.Item2);
                }
            }
            catch (Exception e)
            {
                _dbTransaction.Rollback();
                throw;
            }
            
            _dbTransaction.Commit();
        }
        
        public IList<Tuple<string, string>> GetReplacements()
        {
            var result = new List<Tuple<string, string>>();

            var sql = "SELECT STRING1, STRING2 FROM OUTPUT_REPLACEMENTS";
            var command = new SQLiteCommand(sql, _dbConnection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var str = new Tuple<string, string> (reader["STRING1"].ToString(), reader["STRING2"].ToString());
                result.Add(str);
            }
            reader.Close();
            return result;
        }

        public void DeleteReplacements()
        {
            var sql = "DELETE FROM OUTPUT_REPLACEMENTS";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.ExecuteNonQuery();
        }

        public void InsertConfig(string key, string value)
        {
            var sql = "INSERT INTO PROJECT_CONFIG(KEY, VAL) VALUES (@key, @value)";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value);
            command.ExecuteNonQuery();
        }

        public string GetConfigValue(string key)
        {
            var sql = "SELECT VAL FROM PROJECT_CONFIG WHERE KEY = @key";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@key", key);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var result = reader["VAL"].ToString();
                reader.Close();
                return result;
            }

            return string.Empty;
        }

        public void UpdateConfigValue(string key, string value)
        {
            var sql = "UPDATE PROJECT_CONFIG SET VAL = @value WHERE KEY = @key";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value);
            command.ExecuteNonQuery();
        }

        public void InsertString(TFString str)
        {
            var sql = "INSERT INTO TRANSLATION(VISIBLE, FIELD, OFFSET, ORIGINAL, TRANSLATION) VALUES (@visible, @field, @offset, @original, @translation)";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@visible", str.Visible?1:0);
            command.Parameters.AddWithValue("@field", str.Section);
            command.Parameters.AddWithValue("@offset", str.Offset);
            command.Parameters.AddWithValue("@original", str.Original);
            command.Parameters.AddWithValue("@translation", str.Translation);
            command.ExecuteNonQuery();

            str.Id = GetStrId();
        }

        public void InsertStrings(IList<TFString> strings)
        {
            _dbTransaction = _dbConnection.BeginTransaction();
            try
            {
                foreach (var str in strings)
                {
                    InsertString(str);
                }
            }
            catch (Exception e)
            {
                _dbTransaction.Rollback();
                throw;
            }
            
            _dbTransaction.Commit();
        }

        private long GetStrId()
        {
            var sql = "select last_insert_rowid()";
            var command = new SQLiteCommand(sql, _dbConnection);

            // The row ID is a 64-bit value - cast the Command result to an Int64.
            //
            return (long)command.ExecuteScalar();
        }

        public void UpdateString(long id, string translation)
        {
            var sql = "UPDATE TRANSLATION SET TRANSLATION = @translation WHERE ID = @id";
            var command = new SQLiteCommand(sql, _dbConnection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@translation", translation);
            command.ExecuteNonQuery();
        }

        public void UpdateStrings(IList<TFString> strings)
        {
            _dbTransaction = _dbConnection.BeginTransaction();
            try
            {
                foreach (var str in strings)
                {
                    UpdateString(str.Id, str.Translation);
                }
            }
            catch (Exception e)
            {
                _dbTransaction.Rollback();
                throw;
            }
            
            _dbTransaction.Commit();
        }

        public IList<TFString> GetStrings()
        {
            var result = new List<TFString>();

            var sql = "SELECT ID, VISIBLE, FIELD, OFFSET, ORIGINAL, TRANSLATION FROM TRANSLATION";
            var command = new SQLiteCommand(sql, _dbConnection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var str = new TFString
                {
                    Id = Convert.ToInt64(reader["ID"]),
                    Visible = Convert.ToInt32(reader["VISIBLE"]) == 1,
                    Section = reader["FIELD"].ToString(),
                    Offset = Convert.ToInt32(reader["OFFSET"]),
                    Original = reader["ORIGINAL"].ToString(),
                    Translation = reader["TRANSLATION"].ToString()
                };

                result.Add(str);
            }
            reader.Close();
            return result;
        }
    }
}

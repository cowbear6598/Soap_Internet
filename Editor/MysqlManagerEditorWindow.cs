using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Soap.Internet
{
    public class MysqlManagerEditorWindow : EditorWindow
    {
        private string fileName = "MysqlManagerSetting.asset";

        // 序列化對象
        private SerializedObject serializedObject;
        
        // 初始網域
        private string defalutDomainName = "http://127.0.0.1";
        
        // 初始 timeout 時間
        private int defaultTimeout = 15;

        [SerializeField] private int timeout;

        // 使用到的網域列表
        [SerializeField] private List<string> domainList;
        private SerializedProperty domainListProperty;

        [MenuItem("Soap/MysqlSetting")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MysqlManagerEditorWindow));
        }

        private void Awake()
        {
            // 將此類序列化
            serializedObject = new SerializedObject(this);

            // 獲取這類別可序列化的屬性
            domainList = new List<string>();
            
            domainListProperty = serializedObject.FindProperty("domainList");
            
            if (PlayerPrefs.GetInt("MysqlManager_DomainCount") > 0)
            {
                for (int i = 0; i < PlayerPrefs.GetInt("MysqlManager_DomainCount"); i++)
                {
                    domainList.Add(string.IsNullOrEmpty(PlayerPrefs.GetString("MysqlManager_Domain" + i)) ? defalutDomainName : PlayerPrefs.GetString("MysqlManager_Domain" + i));
                }
            }
            else
            {
                domainList.Add(defalutDomainName);
            }

            timeout = PlayerPrefs.GetInt("MysqlManager_Timeout") == 0 ? PlayerPrefs.GetInt("MysqlManager_Timeout") : defaultTimeout;
        }

        private void OnGUI()
        {
            GUILayout.Label("初始化以及設定 Mysql 連線資訊", EditorStyles.boldLabel);

            // 獲取面板上最新資訊
            serializedObject.Update();
            
            // 開始確認是否有更改
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(domainListProperty, true);
            timeout = EditorGUILayout.IntField("Timeout", timeout);

            // 結束確認是否更改並更新 GUI 等
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
                
            if (GUILayout.Button("初始化與設定"))
            {
                CreateOrSetDomain();
            }
        }

        private void CreateOrSetDomain()
        {
            MysqlManagerScriptableObject mysqlManagerSO = null;
                
            if (File.Exists("Assets/Resources/" + fileName))
            { 
                mysqlManagerSO = (MysqlManagerScriptableObject) EditorGUIUtility.Load("Assets/Resources/" + fileName);

                mysqlManagerSO.domainList = domainList;
                mysqlManagerSO.timeout = timeout;
            }
            else
            {
                mysqlManagerSO = CreateInstance<MysqlManagerScriptableObject>();

                mysqlManagerSO.domainList = domainList;
                mysqlManagerSO.timeout = timeout;

                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                    
                AssetDatabase.CreateAsset(mysqlManagerSO, "Assets/Resources/" + fileName);
            }

            EditorUtility.FocusProjectWindow();
                
            Selection.activeObject = mysqlManagerSO;

            PlayerPrefs.SetInt("MysqlManager_DomainCount", domainList.Count);
            
            for (int i = 0; i < domainList.Count; i++)
            {
                PlayerPrefs.SetString("MysqlManager_Domain" + i, domainList[i]);
            }

            PlayerPrefs.SetInt("MysqlManager_Timeout", timeout);
            
            PlayerPrefs.Save();
                
            EditorUtility.SetDirty(mysqlManagerSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
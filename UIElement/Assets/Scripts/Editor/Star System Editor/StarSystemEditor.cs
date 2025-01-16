using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(StarSystem))]
public class StarSystemEditor : Editor
{
    #region 方法

    public void OnEnable()
    {
        m_starSystem = (StarSystem)target;
        m_rootElement = new VisualElement();

        VisualTreeAsset visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/Editor/Star System Editor/StarSystemEditor.uxml");
        visualTree.CloneTree(m_rootElement);

        StyleSheet stylesheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Star System Editor/StarSystemEditor.uss");
        m_rootElement.styleSheets.Add(stylesheet);
    }

    public override VisualElement CreateInspectorGUI()
    {
        #region Fields

        VisualElement systemSprite = m_rootElement.Query<VisualElement>("systemSprite").First();
        systemSprite.style.backgroundImage = m_starSystem.m_sprite ? m_starSystem.m_sprite.texture : null;

        // 1. 显示Sprite
        ObjectField spriteField = m_rootElement.Query<ObjectField>("systemSpriteField").First();
        spriteField.objectType = typeof(Sprite);
        spriteField.value = m_starSystem.m_sprite;
        spriteField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                m_starSystem.m_sprite = (Sprite)e.newValue;
                systemSprite.style.backgroundImage = m_starSystem.m_sprite.texture;
                EditorUtility.SetDirty(m_starSystem);
            }
        );

        // 2. 显示Scale
        FloatField scaleField = m_rootElement.Query<FloatField>("starScale").First();
        scaleField.value = m_starSystem.m_scale;
        scaleField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                m_starSystem.m_scale = e.newValue;
                EditorUtility.SetDirty(m_starSystem);
            }
        );

        #endregion

        #region 显示Planet数据

        m_planetList = m_rootElement.Q<VisualElement>("planetList");
        UpdatePlanets();

        #endregion

        #region 按钮事件 

        Button btnAddPlanet = m_rootElement.Q<Button>("btnAddNew");
        btnAddPlanet.clickable.clicked += AddPlanet;

        Button btnRemoveAllPlanets = m_rootElement.Q<Button>("btnRemoveAll");
        btnRemoveAllPlanets.clickable.clicked += RemoveAll;

        #endregion

        return m_rootElement;
    }


    /// <summary>
    /// 添加一个Planet
    /// </summary>
    private void AddPlanet()
    {
        Planet planet = CreateInstance<Planet>();
        planet.name = "New Planet";
        m_starSystem.m_planets.Add(planet);
        AssetDatabase.AddObjectToAsset(planet, m_starSystem);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UpdatePlanets();
    }

    /// <summary>
    /// 移除所有的Planet
    /// </summary>
    private void RemoveAll()
    {
        if (EditorUtility.DisplayDialog("Delete All Planets",
                "Are you sure you want to delete all of the planets this star system has?", "Delete All", "Cancel"))
        {
            for (int i = m_starSystem.m_planets.Count - 1; i >= 0; i--)
            {
                AssetDatabase.RemoveObjectFromAsset(m_starSystem.m_planets[i]);
                m_starSystem.m_planets.RemoveAt(i);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdatePlanets();
        }
    }

    /// <summary>
    /// 移除一个Planet
    /// </summary>
    /// <param name="planet">需要移除的Planet</param>
    public void RemovePlanet(Planet planet)
    {
        m_starSystem.m_planets.Remove(planet);
        AssetDatabase.RemoveObjectFromAsset(planet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UpdatePlanets();
    }

    /// <summary>
    /// 更新Planet容器列表
    /// </summary>
    public void UpdatePlanets()
    {
        m_planetList.Clear();
        // 创建并为planetList容器中的每个星球添加一个PlanetSubEditor。
        foreach (Planet planet in m_starSystem.m_planets)
        {
            PlanetSubEditor planetSubEditor = new PlanetSubEditor(this, planet);
            m_planetList.Add(planetSubEditor);
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  StarSystem对象引用
    /// </summary>
    private StarSystem m_starSystem;

    /// <summary>
    /// 显示StarSystem的VisualElement
    /// </summary>
    private VisualElement m_rootElement;

    /// <summary>
    /// 显示多个Planet的VisualElement
    /// </summary>
    private VisualElement m_planetList;

    #endregion
}
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetSubEditor : VisualElement
{
    public PlanetSubEditor(StarSystemEditor starSystemEditor, Planet planet)
    {
        this.m_starSystemEditor = starSystemEditor;
        this.m_planet = planet;

        VisualTreeAsset visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/Editor/Star System Editor/PlanetSubEditor.uxml");
        visualTree.CloneTree(this);

        StyleSheet stylesheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Star System Editor/PlanetSubEditor.uss");
        styleSheets.Add(stylesheet);

        AddToClassList("planetSubeditor");

        #region Fields

        TextField nameField = this.Query<TextField>("planetName").First();
        nameField.value = planet.name;
        nameField.RegisterCallback<ChangeEvent<string>>(e =>
            {
                planet.name = (string)e.newValue;
                EditorUtility.SetDirty(planet);
            }
        );

        // Sprite is displayed the same way as in the Star System Inspector
        VisualElement planetSpriteDisplay = this.Query<VisualElement>("planetSpriteDisplay").First();
        planetSpriteDisplay.style.backgroundImage = planet.m_sprite ? planet.m_sprite.texture : null;

        ObjectField spriteField = this.Query<ObjectField>("planetSprite").First();
        spriteField.objectType = typeof(Sprite);
        spriteField.value = planet.m_sprite;
        spriteField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                planet.m_sprite = (Sprite)e.newValue;
                planetSpriteDisplay.style.backgroundImage = planet.m_sprite.texture;
                EditorUtility.SetDirty(planet);
            }
        );

        FloatField scaleField = this.Query<FloatField>("planetScale").First();
        scaleField.value = planet.m_scale;
        scaleField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                planet.m_scale = e.newValue;
                EditorUtility.SetDirty(planet);
            }
        );

        FloatField distanceField = this.Query<FloatField>("planetDistance").First();
        distanceField.value = planet.m_distance;
        distanceField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                planet.m_distance = e.newValue;
                EditorUtility.SetDirty(planet);
            }
        );


        FloatField speedField = this.Query<FloatField>("planetSpeed").First();
        speedField.value = planet.m_speed;
        speedField.RegisterCallback<ChangeEvent<float>>(e =>
            {
                planet.m_speed = e.newValue;
                EditorUtility.SetDirty(planet);
            }
        );

        #endregion

        #region Buttons

        Button btnAddPlanet = this.Query<Button>("btnRemove").First();
        btnAddPlanet.clickable.clicked += RemovePlanet;

        #endregion
    }

    #region Button Functions

    /// <summary>
    /// 移除一个Planet
    /// </summary>
    private void RemovePlanet()
    {
        if (EditorUtility.DisplayDialog("Delete Planet", "Are you sure you want to delete this planet?", "Delete",
                "Cancel"))
        {
            m_starSystemEditor.RemovePlanet(m_planet);
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 当前的Planet
    /// </summary>
    private Planet m_planet;
    
    /// <summary>
    /// StarSystemEditor 引用，用于删除Planet
    /// </summary>
    private StarSystemEditor m_starSystemEditor;

    #endregion
}
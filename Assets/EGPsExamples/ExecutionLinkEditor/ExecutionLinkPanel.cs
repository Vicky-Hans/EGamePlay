using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EGamePlay.Combat;

namespace EGamePlay
{
    public class ExecutionLinkPanel : MonoBehaviour
    {
        public static ExecutionLinkPanel Instance { get; set; }
        public Text SkillTimeText;
        public Text SkillNameText;
        public Text SkillDescText;
        public Image SkillTimeImage;
        public Transform TimeCursorTrm;
        [Space(10)]
        public Transform FrameInfosContentTrm;
        public Transform FrameTrm;
        public Transform FrameTextTrm;
        public Vector2 FrameTextPos { get; set; }
        [Space(10)]
        public Transform TrackListTrm;
        public Transform TrackTrm;
        [Space(10)]
        public Transform RightContextTrm;
        public Button NewExecutionBtn;
        public Button AddClipBtn;
        public Button SaveBtn;
        public Button DeleteClipBtn;
        public Toggle PauseToggle;
        public Button PlayButton;
        public Button ReloadButton;
        public Button StepBtn;
        public Transform ContentTrm;
        public Transform Button;
        public int CurrentSkillId { get; set; }
        public float TotalTime { get; set; }
        public float CurrentTime { get; set; }
        public int NextActionIndex { get; set; }
        public float PanelWidth { get; set; }
        public bool IsPlaying { get; set; }
        public string CurrentExecutionAssetPath { get; set; }
        public ExecutionObject CurrentExecutionObject { get; set; }
        public ExecuteClipData CurrentExecutionClip { get; set; }
        public CombatEntity HeroEntity { get; set; }
        public CombatEntity BossEntity { get; set; }
        void Start()
        {
            Instance = this;

            var r = GetComponent<RectTransform>();
            PanelWidth = Screen.width - r.offsetMin.x + r.offsetMax.x;
            TrackTrm.SetParent(null);

            FrameTextPos = FrameTextTrm.GetComponent<RectTransform>().localPosition;
            FrameTrm.SetParent(null);
            FrameTextTrm.SetParent(null);

            SkillTimeImage.fillAmount = 0;
            TimeCursorTrm.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);

            PlayButton.onClick.AddListener(PlaySkillExecution);

            NewExecutionBtn.onClick.AddListener(NewExecutionAsset);
            AddClipBtn.onClick.AddListener(AddClipAsset);
            DeleteClipBtn.onClick.AddListener(DeleteClipAsset);
            SaveBtn.onClick.AddListener(SaveAsset);

            Invoke(nameof(AfterStart), 0.1f);
        }
        private void AfterStart()
        {
            Monster.Boss.MotionComponent.Enable = false;
            Monster.Boss.AnimationComponent.Speed = 1;
            Monster.Boss.AnimationComponent.TryPlayFade(Monster.Boss.AnimationComponent.IdleAnimation);
        }
        
        void Update()
        {
            if (IsPlaying)
            {
                CurrentTime += Time.deltaTime;

                var perc = Time.deltaTime / TotalTime;
                SkillTimeImage.fillAmount += perc;
                TimeCursorTrm.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(SkillTimeImage.fillAmount * PanelWidth, 0, 0);

                if (SkillTimeImage.fillAmount >= 1)
                {
                    IsPlaying = false;
                    PlayButton.GetComponentInChildren<Text>().text = "����";
                }
            }

            if (Input.GetMouseButtonUp((int)UnityEngine.UIElements.MouseButton.LeftMouse))
            {
                RightContextTrm.gameObject.SetActive(false);
            }
        }

        private void NewExecutionAsset()
        {
#if UNITY_EDITOR
            var assetName = "Execution_";
            var i = 0;
            while (true)
            {
                var newAssetName = assetName;
                if (i > 0)
                {
                    newAssetName = assetName + i;
                }
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionObject>($"Assets/{newAssetName}.asset");
                if (asset == null)
                {
                    assetName = newAssetName;
                    break;
                }
                i++;
            }

            CurrentExecutionAssetPath = $"Assets/{assetName}.asset";
            var excObj = ScriptableObject.CreateInstance<ExecutionObject>();
            excObj.TotalTime = 1.5f;
            excObj.AbilityId = i;
            UnityEditor.AssetDatabase.CreateAsset(excObj, CurrentExecutionAssetPath);
            SkillListPanel.Instance.RefreshList();
            LoadCurrentSkill();
#endif
        }

        private void AddClipAsset()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(CurrentExecutionAssetPath))
            {
                return;
            }
            var excObj = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionObject>(CurrentExecutionAssetPath);
            var excClipObj = ScriptableObject.CreateInstance<ExecuteClipData>();
            excClipObj.name = "ExecuteClip";
            excClipObj.ExecuteClipType = ExecuteClipType.ItemExecute;
            excClipObj.ItemData = new ItemExecute();
            excClipObj.GetClipTime().EndTime = 0.1f;
            excObj.ExecuteClips.Add(excClipObj);
            UnityEditor.AssetDatabase.AddObjectToAsset(excClipObj, excObj);
            UnityEditor.EditorUtility.SetDirty(excObj);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(excObj);
            LoadCurrentSkill();
#endif
        }

        private void DeleteClipAsset()
        {
#if UNITY_EDITOR
            CurrentExecutionObject.ExecuteClips.Remove(CurrentExecutionClip);
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(CurrentExecutionClip);
            UnityEditor.EditorUtility.SetDirty(CurrentExecutionObject);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(CurrentExecutionObject);
            LoadCurrentSkill();
            RightContextTrm.gameObject.SetActive(false);
#endif
        }

        private void SaveAsset()
        {
#if UNITY_EDITOR
            if (CurrentExecutionObject == null)
            {
                return;
            }
            var dels = new List<Object>();
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(CurrentExecutionAssetPath);
            foreach (var item in assets)
            {
                if (UnityEditor.AssetDatabase.IsMainAsset(item)) continue;
                if (item is ExecuteClipData data && CurrentExecutionObject.ExecuteClips.Contains(data)) continue;
                dels.Add(item);
            }
            foreach (var item in dels)
            {
                Debug.Log($"RemoveObjectFromAsset {item.name}");
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(item);
            }
            UnityEditor.EditorUtility.SetDirty(CurrentExecutionObject);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(CurrentExecutionObject);
#endif
        }

        T Load<T>(string asset) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset);
#else
			return null;
#endif
        }

        private static void DestroyChildren(Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        public void LoadSkill(string path)
        {
            CurrentExecutionAssetPath = path;
            LoadCurrentSkill();
        }

        public void LoadCurrentSkill()
        {
#if UNITY_EDITOR
            var self = this;
            DestroyChildren(self.TrackListTrm);

            CurrentExecutionObject = UnityEditor.AssetDatabase.LoadAssetAtPath<ExecutionObject>(CurrentExecutionAssetPath);

            self.TotalTime = (float)CurrentExecutionObject.TotalTime;

            foreach (var item in CurrentExecutionObject.ExecuteClips)
            {
                if (item.ExecuteClipType == ExecuteClipType.ItemExecute)
                {
                    self.LoadCurrentSkillCollisionClip(item);
                }
                if (item.ExecuteClipType == ExecuteClipType.ActionEvent)
                {
                    self.LoadCurrentSkillActionEvent(item);
                }
                if (item.ExecuteClipType == ExecuteClipType.Animation)
                {
                    self.LoadCurrentSkillAnimation(item);
                }
                if (item.ExecuteClipType == ExecuteClipType.ParticleEffect)
                {
                    self.LoadCurrentSkillParticleEffect(item);
                }
            }

            var trackListSize = self.TrackListTrm.RectTransform().sizeDelta;
            var space = self.TrackListTrm.GetComponent<VerticalLayoutGroup>().spacing;
            self.TrackListTrm.RectTransform().sizeDelta = new Vector2(trackListSize.x, self.TrackListTrm.childCount * (self.TrackTrm.RectTransform().sizeDelta.y + space));

            DestroyChildren(self.FrameInfosContentTrm);

            if (!(self.TotalTime > 0)) return;
            var frameCount = (int)(self.TotalTime * 100);
            for (var i = 0; i < frameCount; i++)
            {
                var frameObj = Instantiate(self.FrameTrm, self.FrameInfosContentTrm);
                if (i % 5 != 0)
                {
                    var c = frameObj.GetComponent<Image>().color;
                    frameObj.GetComponent<Image>().color = new Color(c.r, c.g, c.b, .5f);
                    var r = frameObj.RectTransform();
                    r.sizeDelta = new Vector2(r.sizeDelta.x, r.sizeDelta.y * 0.4f);
                }
                else
                {
                    if (i % 10 != 0)
                    {
                        var c = frameObj.GetComponent<Image>().color;
                        var r = frameObj.RectTransform();
                        r.sizeDelta = new Vector2(r.sizeDelta.x, r.sizeDelta.y * 0.8f);
                    }
                    else
                    {
                        var textObj = Instantiate(self.FrameTextTrm, frameObj.transform);
                        textObj.RectTransform().localPosition = self.FrameTextPos;
                        var milis = i * 10;
                        var secs = milis / 1000;
                        var secs2 = milis % 1000 / 10;
                        textObj.GetComponent<Text>().text = $"{secs}:{secs2.ToString().PadLeft(2, '0')}";
                    }
                }
            }
#endif
        }

        void LoadCurrentSkillCollisionClip(ExecuteClipData trackClipData)
        {
            var self = this;
            var animTrack = Instantiate(self.TrackTrm, self.TrackListTrm, true);
            animTrack.GetComponentInChildren<Text>().text = "collision execute";

            trackClipData.TotalTime = self.TotalTime;

            var trackClip = animTrack.GetComponentInChildren<TrackClip>();
            trackClip.SetClipType(trackClipData);
        }

        void LoadCurrentSkillAnimation(ExecuteClipData trackClipData)
        {
            var self = this;
            var anim = "anim";
            if (string.IsNullOrEmpty(anim))
            {
                return;
            }
            if (trackClipData.AnimationData.AnimationClip != null)
            {
                anim = trackClipData.AnimationData.AnimationClip.name;
            }
            var animTrack = GameObject.Instantiate(self.TrackTrm);
            animTrack.SetParent(self.TrackListTrm);
            animTrack.GetComponentInChildren<Text>().text = $"animation clip : {anim}";

            trackClipData.TotalTime = self.TotalTime;

            var trackClip = animTrack.GetComponentInChildren<TrackClip>();
            trackClip.SetClipType(trackClipData);
        }

        void LoadCurrentSkillParticleEffect(ExecuteClipData trackClipData)
        {
            var self = this;

            var name = "";
            if (trackClipData.ParticleEffectData.ParticleEffect != null)
            {
                name = trackClipData.ParticleEffectData.ParticleEffect.name;
            }
            var animTrack = GameObject.Instantiate(self.TrackTrm);
            animTrack.SetParent(self.TrackListTrm);
            animTrack.GetComponentInChildren<Text>().text = $"{name}";

            trackClipData.TotalTime = self.TotalTime;

            var trackClip = animTrack.GetComponentInChildren<TrackClip>();
            trackClip.SetClipType(trackClipData);
        }

        void LoadCurrentSkillSound()
        {
            var self = this;
            var sound = "sound";
            if (string.IsNullOrEmpty(sound)) return;
            var audio = Load<AudioClip>(sound);
            var animTrack = Instantiate(self.TrackTrm, self.TrackListTrm, true);
            animTrack.GetComponentInChildren<Text>().text = $"{sound}";

            var trackClipData = ScriptableObject.CreateInstance<ExecuteClipData>();
            trackClipData.TotalTime = self.TotalTime;
            trackClipData.ExecuteClipType = ExecuteClipType.Audio;
            trackClipData.AudioData = new AudioData {AudioClip = audio };
            trackClipData.StartTime = 0;
            trackClipData.EndTime = audio.length;

            var trackClip = animTrack.GetComponentInChildren<TrackClip>();
            trackClip.SetClipType(trackClipData);
            trackClip.SetDragEvent();
            trackClip.SliderRight.value = audio.length;
            trackClip.ClipTypeBar.color = new Color(204 / 255f, 130 / 255f, 0f, 1);
        }

        void LoadCurrentSkillActionEvent(ExecuteClipData trackClipData)
        {
            var self = this;
            var startTime = trackClipData.StartTime;

            var actionTrack = Instantiate(self.TrackTrm, self.TrackListTrm, true);
            if (trackClipData.ActionEventData != null)
            {
                actionTrack.GetComponentInChildren<Text>().text = "";
            }

            var trackClip = actionTrack.GetComponentInChildren<TrackClip>();

            trackClip.SliderLeft.value = (float)startTime / self.TotalTime;
            trackClip.SliderRight.value = (float)startTime / self.TotalTime + 0.01f;
            trackClipData.TotalTime = self.TotalTime;
            trackClip.DisableSlider();
            trackClip.SetClipType(trackClipData);
        }

        private void PlaySkillExecution()
        {
            if (CurrentExecutionObject == null) return;
            SkillTimeImage.fillAmount = 0;
            CurrentTime = 0;
            IsPlaying = true;
            if (CurrentExecutionObject.AbilityId > 0 && HeroEntity.GetComponent<SkillComponent>().IdSkills.TryGetValue(CurrentExecutionObject.AbilityId, out var skillAbility))
            {
                skillAbility.LoadExecution();
                if (CurrentExecutionObject.TargetInputType == ExecutionTargetInputType.Target)
                {
                    HeroEntity.GetComponent<SpellComponent>().SpellWithTarget(skillAbility,
                        skillAbility.ConfigObject.AffectTargetType == SkillAffectTargetType.EnemyTeam ? BossEntity : HeroEntity);
                }
                if (CurrentExecutionObject.TargetInputType == ExecutionTargetInputType.Point)
                {
                    HeroEntity.GetComponent<SpellComponent>().SpellWithPoint(skillAbility, BossEntity.Position);
                }
            }
            else
            {
                HeroEntity.ModelTrans.localRotation = Quaternion.LookRotation(BossEntity.Position - HeroEntity.Position);
                if (CurrentExecutionObject.TargetInputType == ExecutionTargetInputType.Target)
                {
                    var execution = HeroEntity.AddChild<AbilityExecution>(null);
                    execution.ExecutionObject = CurrentExecutionObject;
                    execution.InputTarget = BossEntity;
                    execution.LoadExecutionEffects();
                    execution.BeginExecute();
                }

                if (CurrentExecutionObject.TargetInputType != ExecutionTargetInputType.Point) return;
                {
                    var execution = HeroEntity.AddChild<AbilityExecution>(null);
                    execution.ExecutionObject = CurrentExecutionObject;
                    execution.InputPoint = BossEntity.Position;
                    execution.LoadExecutionEffects();
                    execution.BeginExecute();
                }
            }
        }
    }

    public static class ExecutionLinkPanelEx
    {
        public static RectTransform RectTransform(this Transform transform)
        {
            return transform.GetComponent<RectTransform>();
        }
        public static void DestroyChildren(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}

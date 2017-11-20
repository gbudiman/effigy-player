using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ControllerScript : MonoBehaviour {
  Canvas canvas;
  Button playpause_button;
  Button timestamp;
  Text playpause_button_text;
  Text timestamp_text;
  Image fig_renderer;
  Image interface_occlussion;
  SpriteRenderer dummy_sprite;
  List<Sprite> sprites;
  Slider time_slider;
  Slider volume_slider;
  AudioSource audio_source;
  Dropdown playback_speed;
  int sprites_count;
  public enum PlayState { paused, playing};
  public PlayState play_state;
  float video_time;
  float frame_time;
  bool frame_has_been_updated;
  const float FT = 0.05f;
  bool natural_progression;
  float playback_rate = 1f;
  Transform content;
  Image scroll_content_prefab;

	// Use this for initialization
	void Start () {
    play_state = PlayState.paused;
    canvas = GameObject.FindObjectOfType<Canvas>();
    foreach (Button button in canvas.GetComponentsInChildren<Button>()) {
      switch (button.name) {
        case "PlayPauseButton":
          playpause_button = button;
          break;
        case "Timestamp":
          timestamp = button;
          timestamp_text = timestamp.GetComponentInChildren<Text>();
          break;
      }
    }

    foreach (Image image in canvas.GetComponentsInChildren<Image>()) {
      switch (image.name) {
        case "FigRenderer":
          fig_renderer = image.GetComponentInChildren<Image>();
          break;
        case "InterfaceOcclussion":
          interface_occlussion = image.GetComponentInChildren<Image>();
          break;
      }
    }

    foreach (Slider slider in canvas.GetComponentsInChildren<Slider>()) {
      switch (slider.name) {
        case "TimeSlider":
          time_slider = slider;
          break;
        case "VolumeSlider":
          volume_slider = slider;
          break;
      }
    }

    playpause_button_text = playpause_button.GetComponentInChildren<Text>();
    video_time = 0f;
    frame_time = 0f;
    frame_has_been_updated = false;
    
    natural_progression = false;
    playback_speed = GameObject.FindObjectOfType<Dropdown>();
    audio_source = GameObject.FindObjectOfType<AudioSource>();

    //load_all_sprites();
    //load_figs();
  }

  void load_figs(string filepath) {
    content = GameObject.Find("Content").GetComponent<Transform>();
    scroll_content_prefab = Resources.Load<Image>("ScrollContentPrefab");

    List<int> ws;
    string line = null;
    StreamReader stream_reader = new StreamReader(filepath);

    using (stream_reader) {
      ws = new List<int>();

      do {
        line = stream_reader.ReadLine();
        int index;
        bool can_parse = int.TryParse(line, out index);
        if (can_parse) {
          ws.Add(index);
        }
 
      } while (line != null);

      stream_reader.Close();
    }



    content.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 88);

    int occ = 0;
    int mbase = -180; //60;
    int mult = 100;
    int total_width = 10;

    foreach (int w in ws) {
      Image p = Instantiate<Image>(scroll_content_prefab, content);
      FigClickListener fcl = p.GetComponentInChildren<FigClickListener>();
      Vector3 tp = p.transform.position;
      p.sprite = sprites[w];
      p.transform.position = new Vector3(mbase + (mult * occ), tp.y, tp.z);
      fcl.frame_jump = w;

      occ++;
    }

    total_width += occ * mult;

    if (total_width > 750) {
      content.GetComponent<RectTransform>().sizeDelta = new Vector2(total_width / 2, 88);
    }
  }
  public void load_all_sprites() {
    sprites = new List<Sprite>();

    //foreach (Object res in Resources.LoadAll<Sprite>("disney")) {
    //  sprites.Add((Sprite)res);
    //}

    //foreach (Object res in Resources.LoadAll<AudioClip>("disney")) {
    //  audio_source.clip = (AudioClip) res;
    //}

    InputField infield = GameObject.Find("MovieName").GetComponent<InputField>();
    Text load_status = GameObject.Find("LoadStatus").GetComponentInChildren<Text>();
    string path = Application.dataPath;
    //string rel = Path.GetFullPath(Path.Combine(path, @"../../input/" + infield.text)); // apple"));
    string rel = infield.text;

    load_status.text = "Loading...";
    foreach (string file in System.IO.Directory.GetFiles(rel)) {
      string extension = Path.GetExtension(file);

      switch (extension) {
        case ".jpg":
        case ".jpeg":
          sprites.Add(load_new_sprite(file));
          break;
        case ".wav":
          load_audio_clip(file);
          break;
        case ".txt":
          load_figs(file);
          break;
      }

      
    }
    load_status.text = infield.text;
    sprites_count = sprites.Count;
  }

  void load_audio_clip(string filepath) {
    string www_path = "file://" + filepath;
    Debug.Log(www_path);

    WWW audio_loader = new WWW(www_path);

    while (!audio_loader.isDone) {
      //yield return audio_loader;
    }

    audio_source.clip = audio_loader.GetAudioClip(false, false, AudioType.WAV);
  }

  Texture2D load_texture(string filepath) {
    byte[] bdata;
    Texture2D t2d;
    bdata = File.ReadAllBytes(filepath);
    t2d = new Texture2D(352, 288);
    t2d.LoadImage(bdata);

    return t2d;
  }

  Sprite load_new_sprite(string filepath) {
    Sprite new_sprite = new Sprite();
    Texture2D sprite_texture = load_texture(filepath);
    new_sprite = Sprite.Create(sprite_texture, new Rect(0, 0, sprite_texture.width, sprite_texture.height), new Vector2(0.5f, 0.5f), 100f);

    return new_sprite;
  }

	// Update is called once per frame
	void Update () {
    update_video_frame();
	}

  void update_video_frame() {
    float delta_time = Time.fixedDeltaTime * playback_rate;
    if (play_state == PlayState.playing) {
      
      video_time += delta_time;
      frame_time += delta_time;

      //Debug.Log(frame_time);

      // 20 FPS
      if (frame_time >= FT) {
        frame_time -= FT;
        update_renderer_sprite();
        update_time_slider();
        update_timestamp();
      }
    }
  }

  void update_timestamp() {
    int secs = (int) video_time;
    int min = (secs % 3600) / 60;
    int sec = secs % 60;
    int hour = secs / 3600;

    string s = hour.ToString() + ":" + min.ToString("D2") + ":" + sec.ToString("D2");
    timestamp_text.text = s;
  }

  void update_time_slider() {
    float pos = (float) get_frame_by_time(video_time) / sprites_count;
    time_slider.value = pos;
    natural_progression = true;
  }

  void set_video_frame() {
    _set_video(video_time);
  }

  void set_video_frame(float v) {
    video_time = sprites_count * v * FT;
    frame_time = 0;

    _set_video(video_time);
    
  }

  void _set_video(float video_time) {
    if (natural_progression) {
      natural_progression = false;
    } else {
      audio_source.time = video_time;
    }
    update_renderer_sprite();
  }

  void update_renderer_sprite() {
    int frame_index = get_frame_by_time(video_time);

    if (frame_index < sprites.Count) {
      fig_renderer.sprite = sprites[get_frame_by_time(video_time)];
    } else {
      toggle_playpause();
    }
  }

  int get_frame_by_time(float t) {
    return (int) (t / FT);
  }

  void _play(bool val) {
    if (val) {
      fig_renderer.color = new Color(1f, 1f, 1f);

      if (get_frame_by_time(video_time) >= sprites.Count - 1) {
        video_time = 0;
        audio_source.time = 0;
      }

      play_state = PlayState.playing;
      playpause_button_text.text = "Pause";
      audio_source.Play();
    } else {
      play_state = PlayState.paused;
      playpause_button_text.text = "Play";
      audio_source.Pause();
    }
  }

  public void toggle_playpause() {
    if (play_state == PlayState.paused) {
      _play(true);
    } else {
      _play(false);
    }
  }

  public void jump_to_time() {
    float v = time_slider.value;

    set_video_frame(v);
  }

  public void jump_to_frame(int n) {
    set_video_frame((float) n / sprites_count);
    _play(true);
  }

  public void reset_playback() {
    set_video_frame(0);
  }

  public void update_playback_speed() {
    switch (playback_speed.value) {
      case 0: playback_rate = 0.5f; break;
      case 1: playback_rate = 1f; break;
      case 2: playback_rate = 2f; break;
      case 3: playback_rate = 4f; break;
    }

    audio_source.pitch = playback_rate;
    set_video_frame();
  }

  public void update_audio_volume() {
    audio_source.volume = volume_slider.value;
  }
}

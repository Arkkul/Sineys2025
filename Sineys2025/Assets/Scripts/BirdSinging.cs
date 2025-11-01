using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BirdSinging : MonoBehaviour
{
    [Header("Note Settings")]
    [SerializeField] private string _currentNote;
    [SerializeField] private List<string> _notes;
    [SerializeField] private int _currentNoteId = 0;
    [SerializeField] private float _currentSingRate = 2;
    [SerializeField] private float _noteActiveTime;

    [Header("UI Indicators")]
    [SerializeField] private GameObject _noteIndicatorPrefab;
    [SerializeField] private Transform _notesContainer;
    [SerializeField] private Image _timingIndicator;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _missedColor = Color.red;
    [SerializeField] private Color _correctColor = Color.green;
    [SerializeField] private Color _errorColor = Color.red;
    [SerializeField] private Color _activeNoteColor = Color.blue;
    [SerializeField] private Color _wrongTimingColor = Color.magenta; // Цвет для неправильного времени

    [Header("Timing Settings")]
    [SerializeField] private float _perfectTiming = 0.1f;
    [SerializeField] private float _goodTiming = 0.3f;

    private List<GameObject> _noteIndicators = new List<GameObject>();
    private List<Image> _noteImages = new List<Image>();
    private float _timer;
    private bool _waitingForInput;
    private string _expectedNote;
    private Coroutine _flashCoroutine;
    private int _lastPlayedNoteId = -1;
    private bool _inputCooldown = false; // Защита от спама
    private float _inputCooldownTime = 0.3f; // Время кд после нажатия

    private Darker _darker;

    private void Start()
    {
        InitializeNoteIndicators();
        _timer = _currentSingRate;
        _timingIndicator.color = _normalColor;
        _darker = GetComponent<Darker>();
    }

    private void Update()
    {
        UpdateTimingIndicator();

        if (_waitingForInput)
        {
            CheckForInput();
        }

        // Подсвечиваем текущую ноту синим цветом
        UpdateActiveNoteHighlight();
    }

    private void InitializeNoteIndicators()
    {
        // Очищаем существующие индикаторы
        foreach (Transform child in _notesContainer)
        {
            Destroy(child.gameObject);
        }
        _noteIndicators.Clear();
        _noteImages.Clear();

        // Создаем индикаторы для каждой ноты
        for (int i = 0; i < _notes.Count; i++)
        {
            GameObject indicator = Instantiate(_noteIndicatorPrefab, _notesContainer);
            Text textComponent = indicator.GetComponentInChildren<Text>();
            Image imageComponent = indicator.GetComponent<Image>();

            if (textComponent != null)
                textComponent.text = _notes[i];

            _noteIndicators.Add(indicator);
            _noteImages.Add(imageComponent);

            // Делаем прозрачными будущие ноты
            if (i > _currentNoteId)
            {
                SetIndicatorAlpha(indicator, 0.3f);
            }
        }

        // Устанавливаем начальную подсветку активной ноты
        UpdateActiveNoteHighlight();
    }

    private void SetIndicatorAlpha(GameObject indicator, float alpha)
    {
        Image image = indicator.GetComponent<Image>();
        Text text = indicator.GetComponentInChildren<Text>();

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }

    private void UpdateTimingIndicator()
    {
        if (_timingIndicator != null)
        {
            _timer -= Time.deltaTime;
            float fillAmount = _timer / _currentSingRate;
            _timingIndicator.fillAmount = fillAmount;

            // Меняем цвет в зависимости от времени
            if (fillAmount > 0.7f)
            {
                _timingIndicator.color = _normalColor;
            }
            else if (fillAmount > 0.3f)
            {
                _timingIndicator.color = Color.yellow;
            }
            else
            {
                _timingIndicator.color = Color.red;
            }
        }

        if (_timer <= 0f)
        {
            CheckNote();
            _timer = _currentSingRate;
        }
    }

    public void Sing(string note)
    {
        // Проверяем кд и правильность времени нажатия
        if (_inputCooldown)
        {
            Debug.Log("Слишком быстро! Подожди немного перед следующим нажатием.");
            return;
        }

        if (!_waitingForInput)
        {
            Debug.Log("Не время для ноты! Подожди своего хода.");
            _darker.MakeDarker();
            FlashAllIndicators(_wrongTimingColor);
            StartCoroutine(InputCooldown());
            return;
        }

        print(note);
        _currentNote = note;

        // Подсвечиваем ноту синим в момент нажатия
        if (_currentNoteId < _noteImages.Count && _noteImages[_currentNoteId] != null)
        {
            _noteImages[_currentNoteId].color = _activeNoteColor;
        }

        // Проверяем правильность и timing
        float timingAccuracy = Mathf.Abs(_timer - _currentSingRate);
        EvaluateInput(note, timingAccuracy);

        // Включаем кд после нажатия
        StartCoroutine(InputCooldown());

        Invoke(nameof(ResetCurrentNote), _noteActiveTime);
    }

    private IEnumerator InputCooldown()
    {
        _inputCooldown = true;
        yield return new WaitForSeconds(_inputCooldownTime);
        _inputCooldown = false;
    }

    private void CheckForInput()
    {
        // Здесь можно добавить автоматическую проверку тайминга
        // если игрок не успел нажать вовремя
        if (_timer <= 0f && _waitingForInput)
        {
            MissedNote();
        }
    }

    private void EvaluateInput(string note, float timingAccuracy)
    {
        bool isCorrectNote = note == _expectedNote;

        if (isCorrectNote)
        {
            if (timingAccuracy <= _perfectTiming)
            {
                Debug.Log("PERFECT!");
                _darker.MakeVeryMuchLighter();
                FlashIndicator(_currentNoteId, _correctColor);
            }
            else if (timingAccuracy <= _goodTiming)
            {
                Debug.Log("Good!");
                _darker.MakeMuchLighter();
                FlashIndicator(_currentNoteId, _correctColor);
            }
            else
            {
                Debug.Log("Late but correct");
                _darker.MakeDarker();
                FlashIndicator(_currentNoteId, _correctColor);
            }

            // Удаляем сыгранную ноту
            RemovePlayedNote(_currentNoteId);

            // Переходим к следующей ноте
            _currentNoteId++;
            UpdateNoteDisplay();
            _waitingForInput = false;
        }
        else
        {
            Debug.Log("Wrong note!");
            _darker.MakeDarker();

            FlashIndicator(_currentNoteId, _errorColor);
            // Не увеличиваем currentNoteId при ошибке
        }
    }

    private void MissedNote()
    {
        Debug.Log("Missed note!");
        _darker.MakeDarker();
        FlashIndicator(_currentNoteId, _missedColor);

        // Удаляем пропущенную ноту
        RemovePlayedNote(_currentNoteId);

        _currentNoteId++;
        UpdateNoteDisplay();
        _waitingForInput = false;
    }

    private void RemovePlayedNote(int noteId)
    {
        if (noteId < _noteIndicators.Count && _noteIndicators[noteId] != null)
        {
            // Удаляем индикатор ноты
            Destroy(_noteIndicators[noteId]);
            _noteIndicators[noteId] = null;
            _noteImages[noteId] = null;

            _lastPlayedNoteId = noteId;
        }
    }

    private void FlashIndicator(int index, Color color)
    {
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashCoroutine(index, color));
    }

    private void FlashAllIndicators(Color color)
    {
        StartCoroutine(FlashAllCoroutine(color));
    }

    private IEnumerator FlashAllCoroutine(Color color)
    {
        List<Color> originalColors = new List<Color>();

        // Сохраняем оригинальные цвета
        foreach (var image in _noteImages)
        {
            if (image != null)
            {
                originalColors.Add(image.color);
                image.color = color;
            }
        }

        yield return new WaitForSeconds(0.3f);

        // Восстанавливаем цвета
        for (int i = 0; i < _noteImages.Count; i++)
        {
            if (_noteImages[i] != null && i < originalColors.Count)
            {
                if (_waitingForInput && i == _currentNoteId)
                {
                    _noteImages[i].color = _activeNoteColor;
                }
                else
                {
                    _noteImages[i].color = originalColors[i];
                }
            }
        }
    }

    private IEnumerator FlashCoroutine(int index, Color color)
    {
        if (index < _noteImages.Count && _noteImages[index] != null)
        {
            Image image = _noteImages[index];
            Color originalColor = image.color;

            image.color = color;
            yield return new WaitForSeconds(0.3f);

            // После мигания возвращаем нормальный цвет, если нота еще активна
            if (_waitingForInput && index == _currentNoteId)
            {
                image.color = _activeNoteColor;
            }
            else
            {
                image.color = originalColor;
            }
        }
    }

    private void UpdateActiveNoteHighlight()
    {
        // Подсвечиваем текущую активную ноту синим цветом
        for (int i = 0; i < _noteImages.Count; i++)
        {
            if (_noteImages[i] != null)
            {
                if (_waitingForInput && i == _currentNoteId)
                {
                    _noteImages[i].color = _activeNoteColor;
                }
                else if (i == _currentNoteId)
                {
                    _noteImages[i].color = _normalColor;
                }
            }
        }
    }

    private void UpdateNoteDisplay()
    {
        // Обновляем прозрачность всех индикаторов
        for (int i = 0; i < _noteIndicators.Count; i++)
        {
            if (_noteIndicators[i] != null)
            {
                float alpha = (i == _currentNoteId) ? 1f : 0.3f;
                SetIndicatorAlpha(_noteIndicators[i], alpha);
            }
        }

        // Показываем следующую ноту
        if (_currentNoteId < _notes.Count)
        {
            _expectedNote = _notes[_currentNoteId];
            _waitingForInput = true;
        }
        else
        {

            Debug.Log("Sequence completed!");
            _waitingForInput = false;


            InitializeNoteIndicators();
            _timer = _currentSingRate;
            _timingIndicator.color = _normalColor;

            _currentNoteId = 0;
            _expectedNote = _notes[_currentNoteId];
            _waitingForInput = true;
        }

        // Обновляем подсветку активной ноты
        UpdateActiveNoteHighlight();
    }

    private void CheckNote()
    {
        // Этот метод теперь вызывается автоматически по таймеру
        if (_waitingForInput)
        {
            MissedNote();
        }
        else
        {
            _waitingForInput = true;
            _expectedNote = _notes[_currentNoteId];
        }
    }

    private void ResetCurrentNote()
    {
        _currentNote = "";
    }

    // Метод для сброса игры
    public void ResetGame()
    {
        _currentNoteId = 0;
        _lastPlayedNoteId = -1;
        _timer = _currentSingRate;
        _waitingForInput = false;
        _inputCooldown = false;

        // Переинициализируем индикаторы
        InitializeNoteIndicators();
        UpdateNoteDisplay();
    }

    // Публичный метод для проверки состояния (можно использовать для отладки)
    public string GetGameState()
    {
        return $"Current note: {_currentNoteId}, Waiting: {_waitingForInput}, Cooldown: {_inputCooldown}";
    }
    /* [SerializeField] private string _currentNote;
     [SerializeField] private List<string> _notes;
     [SerializeField] private int _currentNoteId = 0;
     [SerializeField] private float _currentSingRate = 2;
     [SerializeField] private float _noteActiveTime;

     private void Start()
     {
         InvokeRepeating(nameof(CheckNote), _currentSingRate, _currentSingRate);
     }

     public void Sing(string note)
     {
         print(note);
         _currentNote = note;
         Invoke(nameof(ResetCurrentNote), _noteActiveTime);
     }

     private void ResetCurrentNote()
     {
         _currentNote = "";
     }

     private void CheckNote()
     {
         if (_notes[_currentNoteId] == _currentNote)
         {
             Debug.Log("good");
         }
         else
         {
             Debug.Log("bad");
         }
         if(_currentNoteId == _notes.Count - 1)
         {
             _currentNoteId = 0;
         }
     }*/
}
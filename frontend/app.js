let quizState = {
    sessionId: null,
    questions: [],
    currentQuestionIndex: 0,
    userAnswers: {},
    playerName: '',
    difficulty: '',
    questionType: ''
};

const startScreen = document.getElementById('start-screen');
const quizScreen = document.getElementById('quiz-screen');
const resultsScreen = document.getElementById('results-screen');
const startBtn = document.getElementById('start-btn');
const restartBtn = document.getElementById('restart-btn');
const errorMessage = document.getElementById('error-message');
const prevBtn = document.getElementById('prev-btn');
const nextBtn = document.getElementById('next-btn');

startBtn.addEventListener('click', startQuiz);
restartBtn.addEventListener('click', resetQuiz);
prevBtn.addEventListener('click', showPreviousQuestion);
nextBtn.addEventListener('click', handleNextButton);

document.querySelectorAll('.difficulty-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        document.querySelectorAll('.difficulty-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
    });
});

document.querySelectorAll('.type-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        document.querySelectorAll('.type-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
    });
});

async function startQuiz() {
    try {
        // Show loading state on button (only change icon)
        startBtn.disabled = true;
        startBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Start Quiz';
        errorMessage.classList.add('hidden');

        let playerName = document.getElementById('player-name').value.trim();
        
        // Limit name length to 30 characters for display
        if (playerName.length > 30) {
            playerName = playerName.substring(0, 30);
        }
        
        const difficulty = document.querySelector('.difficulty-btn.active').dataset.difficulty;
        const questionType = document.querySelector('.type-btn.active').dataset.type;

        quizState.playerName = playerName;
        quizState.difficulty = difficulty;
        quizState.questionType = questionType;

        let apiUrl = `${CONFIG.API_BASE_URL}/trivia/questions?amount=10`;
        if (difficulty) {
            apiUrl += `&difficulty=${difficulty}`;
        }
        if (questionType) {
            apiUrl += `&type=${questionType}`;
        }

        const response = await fetch(apiUrl);
        
        if (!response.ok) {
            throw new Error('Failed to fetch questions');
        }

        const data = await response.json();
        
        quizState.sessionId = data.sessionId;
        quizState.questions = data.questions;
        quizState.currentQuestionIndex = 0;
        quizState.userAnswers = {};

        // Hide start screen and show quiz
        startScreen.classList.add('hidden');
        quizScreen.classList.remove('hidden');
        
        displayQuestion();
    } catch (error) {
        console.error('Error starting quiz:', error);
        errorMessage.classList.remove('hidden');
        startBtn.disabled = false;
        startBtn.innerHTML = '<i class="fas fa-play mr-2"></i>Start Quiz';
    }
}

function displayQuestion() {
    const question = quizState.questions[quizState.currentQuestionIndex];
    const totalQuestions = quizState.questions.length;
    const questionNumber = quizState.currentQuestionIndex + 1;

    document.getElementById('current-question').textContent = questionNumber;
    document.getElementById('total-questions').textContent = totalQuestions;
    document.getElementById('question-text').textContent = question.question;

    const categoryPill = document.getElementById('category-pill');
    categoryPill.textContent = question.category || 'General Knowledge';

    const difficultyPill = document.getElementById('difficulty-pill');
    const difficulty = question.difficulty || 'unknown';
    difficultyPill.textContent = difficulty.charAt(0).toUpperCase() + difficulty.slice(1);
    
    // Set difficulty pill colors (default to gray if API fails to provide)
    difficultyPill.className = 'px-3 py-1 text-xs font-medium rounded-full';
    if (difficulty === 'easy') {
        difficultyPill.classList.add('bg-green-100', 'text-green-700');
    } else if (difficulty === 'medium') {
        difficultyPill.classList.add('bg-yellow-100', 'text-yellow-700');
    } else if (difficulty === 'hard') {
        difficultyPill.classList.add('bg-red-100', 'text-red-700');
    } else {
        difficultyPill.classList.add('bg-gray-100', 'text-gray-700');
    }

    const progress = (questionNumber / totalQuestions) * 100;
    const progressBar = document.getElementById('progress-bar');
    progressBar.style.width = `${progress}%`;

    const answersContainer = document.getElementById('answers-container');
    answersContainer.innerHTML = '';

    question.answers.forEach((answer, index) => {
        const button = document.createElement('button');
        button.className = 'w-full py-2.5 px-4 text-left rounded-lg border border-gray-300 hover:border-indigo-400 hover:bg-indigo-50 transition-all duration-200 hover:scale-105 font-medium text-gray-700';
        button.textContent = answer;
        
        const selectedAnswer = quizState.userAnswers[question.id];
        if (selectedAnswer === answer) {
            button.classList.remove('border-gray-300', 'text-gray-700');
            button.classList.add('border-indigo-500', 'bg-indigo-50', 'text-indigo-700', 'shadow-sm');
        }

        button.addEventListener('click', () => selectAnswer(question.id, answer));
        answersContainer.appendChild(button);
    });

    prevBtn.disabled = quizState.currentQuestionIndex === 0;
    
    const currentQuestion = quizState.questions[quizState.currentQuestionIndex];
    const hasAnswer = quizState.userAnswers[currentQuestion.id] !== undefined;
    
    if (quizState.currentQuestionIndex === totalQuestions - 1) {
        nextBtn.innerHTML = '<i class="fas fa-check mr-2"></i>Submit';
    } else {
        nextBtn.innerHTML = 'Next<i class="fas fa-arrow-right ml-2"></i>';
    }
    
    nextBtn.disabled = !hasAnswer;
    if (!hasAnswer) {
        nextBtn.title = 'Please select an answer to continue';
    } else {
        nextBtn.title = '';
    }
}

function selectAnswer(questionId, answer) {
    quizState.userAnswers[questionId] = answer;
    displayQuestion();
}

function showPreviousQuestion() {
    if (quizState.currentQuestionIndex > 0) {
        quizState.currentQuestionIndex--;
        displayQuestion();
    }
}

function handleNextButton() {
    const totalQuestions = quizState.questions.length;
    
    // Button is disabled until answer is selected, so no need to check
    if (quizState.currentQuestionIndex === totalQuestions - 1) {
        submitQuiz();
    } else {
        quizState.currentQuestionIndex++;
        displayQuestion();
    }
}

async function submitQuiz() {
    try {
        const answerData = {
            sessionId: quizState.sessionId,
            answers: quizState.userAnswers
        };

        const response = await fetch(`${CONFIG.API_BASE_URL}/trivia/checkanswers`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(answerData)
        });

        if (!response.ok) {
            throw new Error('Failed to submit answers');
        }

        const results = await response.json();
        displayResults(results);
    } catch (error) {
        console.error('Error submitting quiz:', error);
        alert('Error submitting quiz. Please try again.');
    }
}

function displayResults(results) {
    quizScreen.classList.add('hidden');
    resultsScreen.classList.remove('hidden');

    const playerNameDisplay = document.getElementById('player-name-display');
    if (quizState.playerName) {
        playerNameDisplay.textContent = `, ${quizState.playerName}`;
    } else {
        playerNameDisplay.textContent = '';
    }

    document.getElementById('score').textContent = results.score;
    document.getElementById('total').textContent = results.totalQuestions;

    // Score message
    const percentage = (results.score / results.totalQuestions) * 100;
    let message = '';
    if (percentage === 100) {
        message = 'Ahh, You got everything right! 🌟';
    } else if (percentage >= 80) {
        message = 'Kinda good! 🎉';
    } else if (percentage >= 60) {
        message = 'Half-decent! 👍';
    } else if (percentage >= 40) {
        message = 'Not bad-ish! 💪';
    } else {
        message = 'You\'ll do better next time...';
    }
    document.getElementById('score-message').textContent = message;

    // Display detailed results
    const resultsDetails = document.getElementById('results-details');
    resultsDetails.innerHTML = '';

    results.results.forEach((result, index) => {
        const question = quizState.questions[result.questionId];
        
        const resultDiv = document.createElement('div');
        resultDiv.className = `mb-4 p-4 rounded-lg ${result.isCorrect ? 'bg-green-50 border-l-4 border-green-500' : 'bg-red-50 border-l-4 border-red-500'}`;
        
        const iconClass = result.isCorrect ? 'fa-circle-check text-green-600' : 'fa-circle-xmark text-red-600';
        
        resultDiv.innerHTML = `
            <div class="flex items-start gap-3 mb-2">
                <i class="fas ${iconClass} text-lg mt-0.5"></i>
                <div class="flex-1">
                    <div class="font-semibold mb-2 ${result.isCorrect ? 'text-green-900' : 'text-red-900'}">
                        Question ${result.questionId + 1}
                    </div>
                    <div class="text-sm mb-2 text-gray-700">${question.question}</div>
                    ${!result.isCorrect ? `
                        <div class="text-sm mb-1 text-gray-700">
                            <span class="font-semibold">Your answer:</span> ${result.userAnswer || 'Not answered'}
                        </div>
                    ` : ''}
                    <div class="text-sm text-gray-700">
                        <span class="font-semibold">Correct answer:</span> ${result.correctAnswer}
                    </div>
                </div>
            </div>
        `;
        
        resultsDetails.appendChild(resultDiv);
    });
}

function resetQuiz() {
    quizState = {
        sessionId: null,
        questions: [],
        currentQuestionIndex: 0,
        userAnswers: {},
        playerName: '',
        difficulty: '',
        questionType: ''
    };

    resultsScreen.classList.add('hidden');
    startScreen.classList.remove('hidden');
    startBtn.disabled = false;
    startBtn.innerHTML = '<i class="fas fa-play mr-2"></i>Start Quiz';
    errorMessage.classList.add('hidden');

    document.getElementById('player-name').value = '';
    document.querySelectorAll('.difficulty-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.difficulty === '') btn.classList.add('active');
    });
    document.querySelectorAll('.type-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.type === '') btn.classList.add('active');
    });
}

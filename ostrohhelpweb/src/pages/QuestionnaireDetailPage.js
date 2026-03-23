import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";

export default function QuestionnaireDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [currentQuestion, setCurrentQuestion] = useState(0);
  const [answers, setAnswers] = useState({});
  const [completed, setCompleted] = useState(false);

  // Mock questionnaire data
  const questionnairesData = {
    1: {
      id: 1,
      title: "Depression Assessment",
      description: "Comprehensive evaluation of depressive symptoms",
      category: "Mental Health",
      estimatedTime: "15 mins",
      questions: [
        {
          id: 1,
          text: "How often do you feel sad or hopeless?",
          type: "scale",
          options: [
            { value: "never", label: "Never" },
            { value: "rarely", label: "Rarely" },
            { value: "sometimes", label: "Sometimes" },
            { value: "often", label: "Often" },
            { value: "always", label: "Always" },
          ],
        },
        {
          id: 2,
          text: "Do you have trouble sleeping?",
          type: "scale",
          options: [
            { value: "no", label: "No" },
            { value: "mild", label: "Mild" },
            { value: "moderate", label: "Moderate" },
            { value: "severe", label: "Severe" },
          ],
        },
        {
          id: 3,
          text: "How is your energy level?",
          type: "scale",
          options: [
            { value: "very_high", label: "Very High" },
            { value: "high", label: "High" },
            { value: "normal", label: "Normal" },
            { value: "low", label: "Low" },
            { value: "very_low", label: "Very Low" },
          ],
        },
      ],
    },
    2: {
      id: 2,
      title: "Anxiety Scale",
      description: "Quick assessment of anxiety levels",
      category: "Anxiety",
      estimatedTime: "10 mins",
      questions: [
        {
          id: 1,
          text: "Do you feel nervous or anxious?",
          type: "scale",
          options: [
            { value: "not_at_all", label: "Not at all" },
            { value: "mildly", label: "Mildly" },
            { value: "moderately", label: "Moderately" },
            { value: "very", label: "Very much" },
          ],
        },
        {
          id: 2,
          text: "How often do you worry?",
          type: "scale",
          options: [
            { value: "never", label: "Never" },
            { value: "sometimes", label: "Sometimes" },
            { value: "often", label: "Often" },
            { value: "always", label: "Almost always" },
          ],
        },
      ],
    },
  };

  const questionnaire = questionnairesData[id] || questionnairesData[1];
  const question = questionnaire.questions[currentQuestion];
  const progress = ((currentQuestion + 1) / questionnaire.questions.length) * 100;

  const handleAnswer = (value) => {
    setAnswers({ ...answers, [question.id]: value });
  };

  const handleNext = () => {
    if (currentQuestion < questionnaire.questions.length - 1) {
      setCurrentQuestion(currentQuestion + 1);
    } else {
      handleSubmit();
    }
  };

  const handlePrevious = () => {
    if (currentQuestion > 0) {
      setCurrentQuestion(currentQuestion - 1);
    }
  };

  const handleSubmit = () => {
    console.log("Questionnaire answers:", answers);
    setCompleted(true);
  };

  if (completed) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="container-adaptive py-12">
          <div className="max-w-2xl mx-auto">
            <Card>
              <div className="text-center py-12">
                <svg className="w-24 h-24 mx-auto mb-6 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
                <h2 className="text-3xl font-bold text-gray-900 mb-4">
                  Questionnaire Completed!
                </h2>
                <p className="text-gray-600 mb-8">
                  Thank you for completing the {questionnaire.title}. Your responses have been recorded.
                </p>

                <div className="bg-blue-50 rounded-lg p-6 mb-8 text-left">
                  <h3 className="font-bold text-gray-900 mb-4">What's Next?</h3>
                  <ul className="space-y-3 text-sm text-gray-700">
                    <li className="flex gap-3">
                      <span className="text-blue-600 font-bold">✓</span>
                      <span>Your responses will be analyzed by our specialists</span>
                    </li>
                    <li className="flex gap-3">
                      <span className="text-blue-600 font-bold">✓</span>
                      <span>You'll receive personalized recommendations</span>
                    </li>
                    <li className="flex gap-3">
                      <span className="text-blue-600 font-bold">✓</span>
                      <span>Book a consultation with a specialist if needed</span>
                    </li>
                  </ul>
                </div>

                <div className="flex gap-4">
                  <Button
                    variant="primary"
                    size="lg"
                    fullWidth
                    onClick={() => navigate("/questionnaires")}
                  >
                    Back to Questionnaires
                  </Button>
                  <Button
                    variant="secondary"
                    size="lg"
                    fullWidth
                    onClick={() => navigate("/consultations")}
                  >
                    Book Consultation
                  </Button>
                </div>
              </div>
            </Card>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container-adaptive py-12">
        {/* Back Button */}
        <button
          onClick={() => navigate("/questionnaires")}
          className="mb-8 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium transition-colors"
        >
          ← Back to Questionnaires
        </button>

        <div className="max-w-2xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <h1 className="section-title">{questionnaire.title}</h1>
            <p className="text-gray-600">
              Estimated time: <span className="font-medium">{questionnaire.estimatedTime}</span>
            </p>
          </div>

          {/* Progress Bar */}
          <div className="mb-8">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700">
                Question {currentQuestion + 1} of {questionnaire.questions.length}
              </span>
              <span className="text-sm font-medium text-gray-700">{Math.round(progress)}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
              <div
                className="bg-gradient-to-r from-blue-600 to-purple-600 h-full transition-all duration-300"
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>

          {/* Question Card */}
          <Card className="mb-8">
            <div className="mb-8">
              <h2 className="text-2xl font-bold text-gray-900 mb-6">{question.text}</h2>

              {question.type === "scale" && (
                <div className="space-y-3">
                  {question.options.map((option) => (
                    <button
                      key={option.value}
                      onClick={() => handleAnswer(option.value)}
                      className={`w-full p-4 rounded-lg text-left font-medium transition-all border-2 ${
                        answers[question.id] === option.value
                          ? "bg-blue-600 text-white border-blue-600"
                          : "bg-white text-gray-900 border-gray-200 hover:border-blue-500"
                      }`}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Navigation */}
            <div className="flex gap-4">
              <Button
                variant="outline"
                size="lg"
                fullWidth
                onClick={handlePrevious}
                disabled={currentQuestion === 0}
              >
                ← Previous
              </Button>
              <Button
                variant="primary"
                size="lg"
                fullWidth
                onClick={handleNext}
                disabled={!answers[question.id]}
              >
                {currentQuestion === questionnaire.questions.length - 1
                  ? "Submit"
                  : "Next →"}
              </Button>
            </div>
          </Card>

          {/* Tips */}
          <Card className="bg-blue-50 border-0">
            <h3 className="font-bold text-gray-900 mb-3">💡 Tip:</h3>
            <p className="text-sm text-gray-600">
              Answer all questions honestly for the most accurate assessment. There are no right or wrong answers.
            </p>
          </Card>
        </div>
      </div>
    </div>
  );
}

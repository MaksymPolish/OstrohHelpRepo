import React from "react";
import { Link } from "react-router-dom";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";

export default function ResourcesPage() {
  const resources = [
    {
      id: 1,
      title: "Meditation Guide",
      description: "Learn various meditation techniques to reduce stress and anxiety.",
      category: "Mindfulness",
      type: "Article",
      duration: "8 min read",
      icon: "🧘",
    },
    {
      id: 2,
      title: "Sleep Hygiene Tips",
      description: "Practical tips to improve your sleep quality and get better rest.",
      category: "Wellness",
      type: "Article",
      duration: "6 min read",
      icon: "😴",
    },
    {
      id: 3,
      title: "Breathing Exercises",
      description: "Simple breathing techniques you can use anytime to calm your mind.",
      category: "Stress Relief",
      type: "Video",
      duration: "12 min video",
      icon: "💨",
    },
    {
      id: 4,
      title: "Nutrition for Mental Health",
      description: "Discover how diet affects your mental health and mood.",
      category: "Wellness",
      type: "Article",
      duration: "10 min read",
      icon: "🥗",
    },
    {
      id: 5,
      title: "Time Management Strategies",
      description: "Productivity tips for students to manage stress and improve focus.",
      category: "Productivity",
      type: "Course",
      duration: "30 min course",
      icon: "⏰",
    },
    {
      id: 6,
      title: "Building Resilience",
      description: "Learn techniques to build mental resilience and overcome challenges.",
      category: "Mental Health",
      type: "Video",
      duration: "15 min video",
      icon: "💪",
    },
  ];

  const categories = ["All", "Mindfulness", "Wellness", "Stress Relief", "Productivity", "Mental Health"];

  const [selectedCategory, setSelectedCategory] = React.useState("All");

  const filteredResources =
    selectedCategory === "All"
      ? resources
      : resources.filter((resource) => resource.category === selectedCategory);

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container-adaptive py-12">
        {/* Header */}
        <div className="mb-12">
          <h1 className="section-title">Mental Health Resources</h1>
          <p className="section-subtitle">
            Access articles, videos, and courses to support your mental health journey.
          </p>
        </div>

        {/* Category Filter */}
        <div className="mb-12 flex gap-3 flex-wrap">
          {categories.map((category) => (
            <button
              key={category}
              onClick={() => setSelectedCategory(category)}
              className={`px-4 py-2 rounded-lg font-medium transition-colors capitalize ${
                selectedCategory === category
                  ? "bg-blue-600 text-white"
                  : "bg-white text-gray-700 border border-gray-300 hover:border-blue-500"
              }`}
            >
              {category}
            </button>
          ))}
        </div>

        {/* Resources Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredResources.map((resource) => (
            <Card key={resource.id} hoverable>
              <div className="mb-4">
                <div className="text-4xl mb-3">{resource.icon}</div>
                <h3 className="font-bold text-lg text-gray-900 mb-2">{resource.title}</h3>
                <p className="text-sm text-gray-600 mb-4">{resource.description}</p>

                <div className="flex gap-2 mb-4">
                  <span className="inline-block px-3 py-1 bg-blue-50 text-blue-700 text-xs font-medium rounded-full">
                    {resource.type}
                  </span>
                  <span className="inline-block px-3 py-1 bg-gray-100 text-gray-600 text-xs font-medium rounded-full">
                    {resource.duration}
                  </span>
                </div>
              </div>

              <Button variant="outline" fullWidth>
                Read More
              </Button>
            </Card>
          ))}
        </div>

        {/* CTA Section */}
        <div className="bg-gradient-to-r from-blue-600 to-purple-600 text-white rounded-2xl p-8 md:p-12 mt-16 text-center">
          <h2 className="text-2xl md:text-3xl font-bold mb-4">
            Need personalized guidance?
          </h2>
          <p className="text-lg text-blue-100 mb-8">
            Talk to a specialist who can provide tailored advice for your situation.
          </p>
          <Link to="/consultations" className="inline-block">
            <Button className="bg-white text-blue-600 hover:bg-gray-100">
              Book a Consultation
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}

import type { BlogPost, FeaturedPost } from '../types/blog';

export const blogData = {
  featuredPosts: [
    {
      id: '1',
      title: 'Is AI gonna take our jobs?',
      description: '',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/pch68867_expires_30_days.png',
      author: 'be for you',
      isFeatured: true
    }
  ] as FeaturedPost[],

  horizontalPosts: [
    {
      id: '2',
      title: 'Migrating to Linear 101',
      description: 'Linear helps streamline software projects, sprints, tasks, and bug tracking. Here\'s how to get...',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/o8s1kb3h_expires_30_days.png',
    },
    {
      id: '3',
      title: 'Building your API Stack',
      description: 'The rise of RESTful APIs has been met by a rise in tools for creating, testing, and manag...',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/vqs7yiqj_expires_30_days.png',
    }
  ] as BlogPost[],

  gridPosts: [
    {
      id: '4',
      title: 'Bill Walsh leadership lessons',
      description: 'Like to know the secrets of transforming a 2-14 team into a 3x Super Bowl winning Dynasty?',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/on2j8p5i_expires_30_days.png',
    },
    {
      id: '5',
      title: 'PM mental models',
      description: 'Mental models are simple expressions of complex processes or relationships.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/2952pnt4_expires_30_days.png',
    },
    {
      id: '6',
      title: 'What is Wireframing?',
      description: 'Introduction to Wireframing and its Principles. Learn from the best in the industry.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/hmqvmcju_expires_30_days.png',
    },
    {
      id: '7',
      title: 'How collaboration makes us better designers',
      description: 'Collaboration can make our teams stronger, and our individual designs better.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/d7d9r5nw_expires_30_days.png',
    },
    {
      id: '8',
      title: 'Our top 10 Javascript frameworks to use',
      description: 'JavaScript frameworks make development easy with extensive features and functionalities.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/1injdb85_expires_30_days.png',
    },
    {
      id: '9',
      title: 'Podcast: Creating a better CX Community',
      description: 'Starting a community doesn\'t need to be complicated, but how do you get started?',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/m6kvcs30_expires_30_days.png',
    },
    {
      id: '10',
      title: 'The Future of Mobile App Development',
      description: 'Discover the latest trends and technologies shaping the future of mobile applications.',
      date: 'Monday, 15 Jan 2023',
      image: 'https://picsum.photos/seed/mobile1/800/600',
    },
    {
      id: '11',
      title: 'Mastering CSS Grid Layout',
      description: 'Learn how to create complex web layouts with CSS Grid, the most powerful layout system available in CSS.',
      date: 'Wednesday, 18 Jan 2023',
      image: 'https://picsum.photos/seed/cssgrid/800/600',
    },
    {
      id: '12',
      title: 'Introduction to TypeScript for React Developers',
      description: 'Why TypeScript is becoming essential for React projects and how to get started with it.',
      date: 'Friday, 20 Jan 2023',
      image: 'https://picsum.photos/seed/typescript/800/600',
    },
    {
      id: '13',
      title: 'Building Accessible Web Applications',
      description: 'Best practices for creating web applications that everyone can use, regardless of ability.',
      date: 'Monday, 23 Jan 2023',
      image: 'https://picsum.photos/seed/a11y/800/600', 
    },
    {
      id: '14',
      title: 'Getting Started with GraphQL',
      description: 'A beginner\'s guide to GraphQL and why it might be better than REST for your next project.',
      date: 'Thursday, 26 Jan 2023',
      image: 'https://picsum.photos/seed/graphql/800/600',
    },
    {
      id: '15',
      title: 'The Art of Code Review',
      description: 'How to give and receive code reviews that improve code quality and team collaboration.',
      date: 'Sunday, 29 Jan 2023',
      image: 'https://picsum.photos/seed/codereview/800/600',
    },
    {
      id: '16',
      title: 'Optimizing React Applications',
      description: 'Performance optimization techniques for faster and more efficient React applications.',
      date: 'Wednesday, 1 Feb 2023',
      image: 'https://picsum.photos/seed/reactperf/800/600',
    },
    {
      id: '17',
      title: 'Introduction to Docker for Web Developers',
      description: 'Learn how Docker can simplify your development environment setup and deployment processes.',
      date: 'Saturday, 4 Feb 2023',
      image: 'https://picsum.photos/seed/docker/800/600',
    },
    {
      id: '18',
      title: 'Building Responsive Websites with Tailwind CSS',
      description: 'How utility-first CSS frameworks like Tailwind can speed up your development workflow.',
      date: 'Tuesday, 7 Feb 2023',
      image: 'https://picsum.photos/seed/tailwind/800/600',
    }
  ] as BlogPost[]
};

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
      tags: [
        { name: 'Design', color: '#6840C6', backgroundColor: '#F9F5FF' },
        { name: 'Research', color: '#3537CC', backgroundColor: '#EEF3FF' },
        { name: 'Presentation', color: '#C01573', backgroundColor: '#FDF1F9' }
      ],
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
      tags: [
        { name: 'Design', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'Research', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '3',
      title: 'Building your API Stack',
      description: 'The rise of RESTful APIs has been met by a rise in tools for creating, testing, and manag...',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/vqs7yiqj_expires_30_days.png',
      tags: [
        { name: 'Design', color: '#027947', backgroundColor: '#EBFDF2' },
        { name: 'Research', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    }
  ] as BlogPost[],

  gridPosts: [
    {
      id: '4',
      title: 'Bill Walsh leadership lessons',
      description: 'Like to know the secrets of transforming a 2-14 team into a 3x Super Bowl winning Dynasty?',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/on2j8p5i_expires_30_days.png',
      tags: [
        { name: 'Leadership', color: '#6840C6', backgroundColor: '#F9F5FF' },
        { name: 'Management', color: '#353E72', backgroundColor: '#F8F8FB' },
        { name: 'Presentation', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '5',
      title: 'PM mental models',
      description: 'Mental models are simple expressions of complex processes or relationships.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/2952pnt4_expires_30_days.png',
      tags: [
        { name: 'Product', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'Research', color: '#3537CC', backgroundColor: '#EEF3FF' },
        { name: 'Frameworks', color: '#C3320A', backgroundColor: '#FFF5ED' }
      ]
    },
    {
      id: '6',
      title: 'What is Wireframing?',
      description: 'Introduction to Wireframing and its Principles. Learn from the best in the industry.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/hmqvmcju_expires_30_days.png',
      tags: [
        { name: 'Design', color: '#6840C6', backgroundColor: '#F9F5FF' },
        { name: 'Research', color: '#3537CC', backgroundColor: '#EEF3FF' },
        { name: 'Presentation', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '7',
      title: 'How collaboration makes us better designers',
      description: 'Collaboration can make our teams stronger, and our individual designs better.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/d7d9r5nw_expires_30_days.png',
      tags: [
        { name: 'Design', color: '#6840C6', backgroundColor: '#F9F5FF' },
        { name: 'Research', color: '#3537CC', backgroundColor: '#EEF3FF' },
        { name: 'Presentation', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '8',
      title: 'Our top 10 Javascript frameworks to use',
      description: 'JavaScript frameworks make development easy with extensive features and functionalities.',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/1injdb85_expires_30_days.png',
      tags: [
        { name: 'Software Development', color: '#027947', backgroundColor: '#EBFDF2' },
        { name: 'Tools', color: '#C01573', backgroundColor: '#FDF1F9' },
        { name: 'SaaS', color: '#C00F47', backgroundColor: '#FEF1F2' }
      ]
    },
    {
      id: '9',
      title: 'Podcast: Creating a better CX Community',
      description: 'Starting a community doesn\'t need to be complicated, but how do you get started?',
      date: 'Sunday, 1 Jan 2023',
      image: 'https://storage.googleapis.com/tagjs-prod.appspot.com/v1/5u0tUzYz6l/m6kvcs30_expires_30_days.png',
      tags: [
        { name: 'Podcasts', color: '#6840C6', backgroundColor: '#F9F5FF' },
        { name: 'Customer Success', color: '#353E72', backgroundColor: '#F8F8FB' },
        { name: 'Presentation', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '10',
      title: 'The Future of Mobile App Development',
      description: 'Discover the latest trends and technologies shaping the future of mobile applications.',
      date: 'Monday, 15 Jan 2023',
      image: 'https://picsum.photos/seed/mobile1/800/600',
      tags: [
        { name: 'Mobile', color: '#027947', backgroundColor: '#EBFDF2' },
        { name: 'Development', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '11',
      title: 'Mastering CSS Grid Layout',
      description: 'Learn how to create complex web layouts with CSS Grid, the most powerful layout system available in CSS.',
      date: 'Wednesday, 18 Jan 2023',
      image: 'https://picsum.photos/seed/cssgrid/800/600',
      tags: [
        { name: 'CSS', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'Web Design', color: '#C01573', backgroundColor: '#FDF1F9' }
      ]
    },
    {
      id: '12',
      title: 'Introduction to TypeScript for React Developers',
      description: 'Why TypeScript is becoming essential for React projects and how to get started with it.',
      date: 'Friday, 20 Jan 2023',
      image: 'https://picsum.photos/seed/typescript/800/600',
      tags: [
        { name: 'TypeScript', color: '#027947', backgroundColor: '#EBFDF2' },
        { name: 'React', color: '#016AA2', backgroundColor: '#EBF8FF' }
      ]
    },
    {
      id: '13',
      title: 'Building Accessible Web Applications',
      description: 'Best practices for creating web applications that everyone can use, regardless of ability.',
      date: 'Monday, 23 Jan 2023',
      image: 'https://picsum.photos/seed/a11y/800/600',
      tags: [
        { name: 'Accessibility', color: '#C3320A', backgroundColor: '#FFF5ED' },
        { name: 'UX', color: '#6840C6', backgroundColor: '#F9F5FF' }
      ]
    },
    {
      id: '14',
      title: 'Getting Started with GraphQL',
      description: 'A beginner\'s guide to GraphQL and why it might be better than REST for your next project.',
      date: 'Thursday, 26 Jan 2023',
      image: 'https://picsum.photos/seed/graphql/800/600',
      tags: [
        { name: 'GraphQL', color: '#C01573', backgroundColor: '#FDF1F9' },
        { name: 'API', color: '#027947', backgroundColor: '#EBFDF2' }
      ]
    },
    {
      id: '15',
      title: 'The Art of Code Review',
      description: 'How to give and receive code reviews that improve code quality and team collaboration.',
      date: 'Sunday, 29 Jan 2023',
      image: 'https://picsum.photos/seed/codereview/800/600',
      tags: [
        { name: 'Best Practices', color: '#353E72', backgroundColor: '#F8F8FB' },
        { name: 'Teamwork', color: '#6840C6', backgroundColor: '#F9F5FF' }
      ]
    },
    {
      id: '16',
      title: 'Optimizing React Applications',
      description: 'Performance optimization techniques for faster and more efficient React applications.',
      date: 'Wednesday, 1 Feb 2023',
      image: 'https://picsum.photos/seed/reactperf/800/600',
      tags: [
        { name: 'React', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'Performance', color: '#C3320A', backgroundColor: '#FFF5ED' }
      ]
    },
    {
      id: '17',
      title: 'Introduction to Docker for Web Developers',
      description: 'Learn how Docker can simplify your development environment setup and deployment processes.',
      date: 'Saturday, 4 Feb 2023',
      image: 'https://picsum.photos/seed/docker/800/600',
      tags: [
        { name: 'Docker', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'DevOps', color: '#027947', backgroundColor: '#EBFDF2' }
      ]
    },
    {
      id: '18',
      title: 'Building Responsive Websites with Tailwind CSS',
      description: 'How utility-first CSS frameworks like Tailwind can speed up your development workflow.',
      date: 'Tuesday, 7 Feb 2023',
      image: 'https://picsum.photos/seed/tailwind/800/600',
      tags: [
        { name: 'CSS', color: '#016AA2', backgroundColor: '#EBF8FF' },
        { name: 'Tailwind', color: '#6840C6', backgroundColor: '#F9F5FF' }
      ]
    }
  ] as BlogPost[]
};
